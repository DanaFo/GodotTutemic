extends Control

@export_node_path var _dialog_text_path
@onready var _dialog_text := get_node(_dialog_text_path)

@export_node_path var _avatarTexturePath
@onready var _avatarTexture := get_node(_avatarTexturePath)

@export var _currentDialogueTres : Dialogue



func _ready():
	_dialog_text.text = _currentDialogueTres.dialogue_slides[0]
	_avatarTexture.texture = _currentDialogueTres.avatar_texture
	pass
