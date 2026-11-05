using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IUHCampus.Guardian.Backend
{
    public sealed class GuardianHttpRequest
    {
        public string Url, Json, TargetClass;
        public byte[] Jpg;
        public int TimeoutSeconds;
    }
    public sealed class GuardianHttpResponse
    {
        public long StatusCode;
        public string Body;
        public bool NetworkError;
    }
    public interface IGuardianHttpTransport
    {
        Awaitable<GuardianHttpResponse> SendAsync(GuardianHttpRequest request, CancellationToken cancellationToken);
    }
    public sealed class UnityWebRequestTransport : IGuardianHttpTransport
    {
        public async Awaitable<GuardianHttpResponse> SendAsync(GuardianHttpRequest input, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var request = Build(input);
            request.timeout = Math.Max(1, input.TimeoutSeconds);
            var operation = request.SendWebRequest();
            var completion = new TaskCompletionSource<bool>();
            void Completed(AsyncOperation _) => completion.TrySetResult(true);
            operation.completed += Completed;
            if (operation.isDone) completion.TrySetResult(true);
            using var cancellation = cancellationToken.Register(() => completion.TrySetCanceled());
            try
            {
                // NextFrameAsync relies on the runtime player loop; completed also runs in Edit Mode.
                await completion.Task;
                cancellationToken.ThrowIfCancellationRequested();
                return new GuardianHttpResponse { StatusCode = request.responseCode,
                    Body = request.downloadHandler.text,
                    NetworkError = request.result == UnityWebRequest.Result.ConnectionError ||
                                   request.result == UnityWebRequest.Result.DataProcessingError };
            }
            catch (OperationCanceledException) { request.Abort(); throw; }
            finally { operation.completed -= Completed; }
        }

        private static UnityWebRequest Build(GuardianHttpRequest input)
        {
            if (input.Jpg != null)
            {
                var fields = new List<IMultipartFormSection> {
                    new MultipartFormFileSection("image", input.Jpg, "frame.jpg", "image/jpeg") };
                if (!string.IsNullOrEmpty(input.TargetClass)) fields.Add(new MultipartFormDataSection("target_class", input.TargetClass));
                return UnityWebRequest.Post(input.Url, fields);
            }
            if (input.Json == null) return UnityWebRequest.Get(input.Url);
            var request = new UnityWebRequest(input.Url, "POST") {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(input.Json)),
                downloadHandler = new DownloadHandlerBuffer() };
            request.SetRequestHeader("Content-Type", "application/json");
            return request;
        }
    }
}
