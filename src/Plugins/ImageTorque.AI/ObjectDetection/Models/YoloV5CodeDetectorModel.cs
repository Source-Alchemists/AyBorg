using ImageTorque.AI.Yolo;

namespace AyBorg.Plugins.ImageTorque.AI;

public record YoloV5CodeDetectorModel() : YoloModel(
    640,
    640,
    3,
    7,
    AnchorsP5D640.Anchors,
    0.20f,
    0.25f,
    0.45f,
    new[] { "output" },
    new List<YoloLabel> {
        new(0, "1d_code"),
        new(1, "2d_code")
    }
);
