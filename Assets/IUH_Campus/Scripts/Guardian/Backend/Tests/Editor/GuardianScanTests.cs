using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
namespace IUHCampus.Guardian.Backend.Tests
{
    public sealed class GuardianScanTests
    {
        private sealed class Transport : IGuardianHttpTransport
        {
            public GuardianHttpResponse Response;
            public GuardianHttpRequest Request;
            public Awaitable<GuardianHttpResponse> SendAsync(GuardianHttpRequest input, CancellationToken token)
            { Request = input; var source = new AwaitableCompletionSource<GuardianHttpResponse>(); source.SetResult(Response); return source.Awaitable; }
        }
        [TestCase(201, "{\"scan_session_id\":\"id\",\"hint\":\"h\",\"expires_at\":\"date\",\"max_monsters\":2}", ScanSessionOutcome.Created)]
        [TestCase(409, "{\"error\":\"scan_session_active\",\"scan_session_id\":\"id\"}", ScanSessionOutcome.ActiveExists)]
        [TestCase(429, "{\"error\":\"scan_cooldown\"}", ScanSessionOutcome.Cooldown)]
        [TestCase(429, "{\"error\":\"scan_limit_reached\"}", ScanSessionOutcome.LimitReached)]
        [TestCase(500, "{}", ScanSessionOutcome.ServerError)]
        [TestCase(200, "bad json", ScanSessionOutcome.ServerError)]
        [TestCase(0, "", ScanSessionOutcome.NetworkError)]
        public async Task SessionMapping(int code, string body, ScanSessionOutcome expected)
        {
            var config = ScriptableObject.CreateInstance<GuardianBackendConfig>();
            try {
                var transport = new Transport { Response = new GuardianHttpResponse { StatusCode = code, Body = body, RetryAfter = "120" } };
                var result = await new GuardianApiClient(config, transport).StartScanSessionAsync();
                Assert.That(result.Outcome, Is.EqualTo(expected));
                StringAssert.Contains(GuardianSession.Id, transport.Request.Json);
                if (expected == ScanSessionOutcome.Cooldown) Assert.That(result.RetryAfterSeconds, Is.EqualTo(120));
                if (expected == ScanSessionOutcome.ActiveExists) Assert.That(result.ScanSessionId, Is.EqualTo("id"));
            } finally { UnityEngine.Object.DestroyImmediate(config); }
        }
        [TestCase(200, "{\"scan\":{\"label\":\"laptop\",\"result\":\"infested\",\"monster_token\":\"tok\",\"monsters_remaining\":1},\"detections\":[{\"label\":\"laptop\",\"confidence\":0.9}],\"inference_time_ms\":12}", ScanFrameOutcome.Infested)]
        [TestCase(200, "{\"scan\":{\"label\":\"chair\",\"result\":\"clean\"}}", ScanFrameOutcome.Clean)]
        [TestCase(200, "{\"scan\":{\"result\":\"none\"}}", ScanFrameOutcome.NoObject)]
        [TestCase(404, "{\"error\":\"scan_session_not_found\"}", ScanFrameOutcome.SessionNotFound)]
        [TestCase(410, "{\"error\":\"scan_session_expired\"}", ScanFrameOutcome.SessionExpired)]
        [TestCase(503, "{}", ScanFrameOutcome.ServerError)]
        [TestCase(200, "{}", ScanFrameOutcome.ServerError)]
        [TestCase(0, "", ScanFrameOutcome.NetworkError)]
        public async Task FrameMapping(int code, string body, ScanFrameOutcome expected)
        {
            var config = ScriptableObject.CreateInstance<GuardianBackendConfig>();
            try {
                var transport = new Transport { Response = new GuardianHttpResponse { StatusCode = code, Body = body } };
                var result = await new GuardianApiClient(config, transport).ScanFrameAsync(new byte[] { 1 }, "id");
                Assert.That(result.Outcome, Is.EqualTo(expected)); Assert.That(transport.Request.ScanSessionId, Is.EqualTo("id"));
                if (expected == ScanFrameOutcome.Infested) { Assert.That(result.MonsterToken, Is.EqualTo("tok")); Assert.That(result.Confidence, Is.EqualTo(.9f)); Assert.That(result.InferenceMs, Is.EqualTo(12)); }
            } finally { UnityEngine.Object.DestroyImmediate(config); }
        }
        [TestCase(200, "{\"scan_session_id\":\"id\",\"expires_at\":\"date\",\"monsters_remaining\":1}", ScanSessionOutcome.Found)]
        [TestCase(404, "{\"error\":\"scan_session_not_found\"}", ScanSessionOutcome.SessionNotFound)]
        [TestCase(410, "{\"error\":\"scan_session_expired\"}", ScanSessionOutcome.SessionExpired)]
        public async Task GetSessionMapping(int code, string body, ScanSessionOutcome expected)
        {
            var config = ScriptableObject.CreateInstance<GuardianBackendConfig>();
            try {
                var transport = new Transport { Response = new GuardianHttpResponse { StatusCode = code, Body = body } };
                var result = await new GuardianApiClient(config, transport).GetScanSessionAsync("id");
                Assert.That(result.Outcome, Is.EqualTo(expected));
                StringAssert.EndsWith("/scan/sessions/id", transport.Request.Url);
                Assert.IsNull(transport.Request.Json);
            } finally { UnityEngine.Object.DestroyImmediate(config); }
        }
        [Test] public void StabilizerAndThrottle()
        {
            var s = new GuardianScanStabilizer(); var f = new ScanFrameResult { Label = "book", Outcome = ScanFrameOutcome.Clean };
            Assert.IsFalse(s.Observe(f, 0)); Assert.IsFalse(s.Observe(f, .5)); Assert.That(s.Progress, Is.InRange(.32f, .34f));
            Assert.IsTrue(s.Observe(f, 1.5)); Assert.IsFalse(s.Observe(f, 2));
            f.Label = "chair"; Assert.IsFalse(s.Observe(f, 2)); Assert.That(s.Progress, Is.Zero);
            f.Outcome = ScanFrameOutcome.Infested; Assert.IsFalse(s.Observe(f, 3)); Assert.That(s.Progress, Is.Zero);
            Assert.IsTrue(s.TryBeginFrame(0)); Assert.IsFalse(s.TryBeginFrame(1)); s.EndFrame(); Assert.IsFalse(s.TryBeginFrame(.4)); Assert.IsTrue(s.TryBeginFrame(.5));
        }
        [Test] public async Task FakeAndToken()
        {
            var fake = new FakeScanService(new[] { "book" }, new[] { "book", "book", "chair" }, "hint");
            var start = await fake.StartScanSessionAsync(); Assert.That(start.Hint, Is.EqualTo("hint"));
            var first = await fake.ScanFrameAsync(null, start.ScanSessionId);
            Assert.That(first.Outcome, Is.EqualTo(ScanFrameOutcome.Infested));
            Assert.That((await fake.ScanFrameAsync(null, start.ScanSessionId)).MonsterToken, Is.EqualTo(first.MonsterToken));
            Assert.That((await fake.ScanFrameAsync(null, start.ScanSessionId)).Outcome, Is.EqualTo(ScanFrameOutcome.Clean));
            Assert.That((await fake.ScanFrameAsync(null, start.ScanSessionId)).Outcome, Is.EqualTo(ScanFrameOutcome.NoObject));
            StringAssert.Contains("\"monster_token\":\"" + first.MonsterToken + "\"", GuardianEventFactory.CombatWon("q", "ar_monster", first.MonsterToken).Json);
            StringAssert.DoesNotContain("payload", GuardianEventFactory.CombatWon("q", "library_intro").Json);
        }
    }
}
