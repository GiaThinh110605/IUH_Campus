# B2 image fixtures

All files are small test inputs, not a representative accuracy benchmark.

| File | Source / author | License / modification |
|---|---|---|
| laptop-stocksnap.jpg | Bonnie Kittle, https://stocksnap.io/photo/laptop-computer-1G9S0K556O ; image https://cdn.stocksnap.io/img-thumbs/960w/laptop-computer_1G9S0K556O.jpg | CC0 1.0, source page explicitly marks CC0. Resized to max 800 px and re-encoded JPEG |
| laptop-isorepublic.jpg | Pixabay via ISO Republic, https://isorepublic.com/photo/laptop-computer-keyboard/ ; image https://isorepublic.com/wp-content/uploads/2018/11/picography-laptop-computer-closeup-minimal-1-1100x689.jpg | CC0, source page explicitly marks CC0. Resized to max 800 px and re-encoded JPEG |
| blank.png | Generated for this project with Pillow, plain white RGB canvas 640x480 | CC0 1.0; synthetic negative, no real scene |
| shapes.png | Generated for this project with Pillow, ellipse/rectangle on navy canvas 640x480 | CC0 1.0; synthetic negative, no real scene |

License: https://creativecommons.org/publicdomain/zero/1.0/
Retrieved/generated 2026-11-02. Positive fixture expectations are verified against the committed real model at threshold 0.70; negatives test absence of laptop, not real-world coverage.
