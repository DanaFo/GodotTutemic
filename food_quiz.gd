extends Node3D

@export_node_path("Area3D") var _quiz_overlap_area3d_path
@onready var _quiz_overlap_area3d := get_node(_quiz_overlap_area3d_path)

@export var _character_texture : Texture2D

@onready var _canvas_layer : CanvasLayer

func _ready() -> void:
	call_deferred("set_canvas_layer") 
	

func set_canvas_layer():
	_canvas_layer = Global.canvas_layer
	if (_canvas_layer):
		print("canvas layer is good")

func _on_dialog_trigger_body_entered(body: Node3D) -> void:
	if (body.name == "Player"):
		#print("Player entered")
		#print("canvas layer name: " + _canvas_layer.name)
		_canvas_layer.visible = false;
		pass # Replace with function body.


func _on_dialog_trigger_body_exited(body: Node3D) -> void:
	if (body.name == "Player"):
		#print("Player exited")
		
		pass # Replace with function body.
