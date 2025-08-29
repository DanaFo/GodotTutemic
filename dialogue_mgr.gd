extends Control

@export_node_path var _dialog_text_path
@onready var _dialog_text := get_node(_dialog_text_path)

@export_node_path var _avatarTexturePath
@onready var _avatarTexture := get_node(_avatarTexturePath)

@export var _currentDialogueTres : Dialogue

var _current_slide_index := 0
var _max_slide_count := 999

func _ready():
	
	_avatarTexture.texture = _currentDialogueTres.avatar_texture
	_max_slide_count = _currentDialogueTres.dialogue_slides.size()
	pass

func _input(event: InputEvent) -> void:
	if (Input.is_action_just_pressed("advance_slide") ):
		if (_current_slide_index < _max_slide_count -1):
			_current_slide_index += 1
			show_slide()
		else:
			self.visible = false
	
	
func show_slide() -> void:
	_dialog_text.text = _currentDialogueTres.dialogue_slides[_current_slide_index]
