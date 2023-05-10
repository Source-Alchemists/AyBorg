using ImageTorque.AI.Yolo;

namespace AyBorg.Plugins.ImageTorque.AI;

public record YoloV5VialDetectorModel() : YoloModel(
    640,
    640,
    3,
    7,
    AnchorsP5D640.Anchors,
    0.20f,
    0.25f,
    0.45f,
    new[] { "output0" },
    new List<YoloLabel> {
        new(0, "Cap"),
        new(1, "Vial")
    }
);
