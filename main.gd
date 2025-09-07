extends Node3D

func _ready():
	Global.set_canvas_layer($CanvasLayer)
	#if (Global.canvas_layer != null)
	#	print("not null")
