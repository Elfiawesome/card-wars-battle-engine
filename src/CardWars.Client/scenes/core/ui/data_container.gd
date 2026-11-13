extends Container
class_name DataContainer

@export var item_scene: PackedScene:
	set(value):
		if item_scene == value:
			return
		item_scene = value
		_rebuild()

@export var item_update_method: StringName = &"set_data"

var _items: Array = []

func set_items(items: Array) -> void:
	_items = items.duplicate()
	_sync()

func set_scene(scene: PackedScene) -> void:
	item_scene = scene

func clear() -> void:
	set_items([])

func get_items() -> Array:
	return _items.duplicate()

func _ready() -> void:
	_sync()

func _rebuild() -> void:
	if not is_node_ready():
		return
	
	for child in get_children():
		remove_child(child)
		child.queue_free()
	_sync()

func _sync() -> void:
	if not is_node_ready():
		return
	
	var target_count := _items.size()
	var children := get_children()
	
	# Resize to make less than target size
	for index in range(children.size() - 1, target_count - 1, -1):
		var child: Node = children[index]
		remove_child(child)
		child.queue_free()
	
	# If smaller than the target size, add
	if get_child_count() < target_count:
		if item_scene == null:
			push_error("DataContainer requires an item_scene to create items.")
			return
		for index in range(get_child_count(), target_count):
			add_child(item_scene.instantiate())
	
	children = get_children()
	
	# Populate data
	for index in range(target_count):
		var child: Node = children[index]
		if not child.has_method(item_update_method):
			push_warning("DataContainer item [%s] is missing method [%s]." % [child.name, item_update_method])
			continue
		child.call(item_update_method, _items[index])