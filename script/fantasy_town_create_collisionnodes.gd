@tool
extends EditorScenePostImport

func _post_import(scene: Node) -> Object:
	recursive_create_collision(scene)
	print("returning scene")
	return scene
	
func recursive_create_collision(object: Node) -> void:
	if object is MeshInstance3D:
		print("is meshinstance 3d")
		object.create_trimesh_collision()
	else: print("bad mesh")
	
	for child in object.get_children():
			print("for child part")
			recursive_create_collision(child)
