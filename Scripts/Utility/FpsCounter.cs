using Godot;
using System;

public partial class FpsCounter : Label
{
	public override void _Process(double delta)
	{
		// Get the current frames per second from the Engine
		double fps = Engine.GetFramesPerSecond();

		// Update the label to show FPS
		Text = $"FPS: {fps:F1}";
	}

}
