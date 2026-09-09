extends Control

@export var camera_node: Camera3D
@export var smooth_speed: float = 10.0
@export var zoom_speed: float = 5.0
@export var zoom_step: float = 0.2

var is_dragging := false
var drag_start_mouse_pos: Vector2
var drag_start_camera_pos: Vector3

var target_position: Vector3
var target_z: float

func _ready() -> void:
	if camera_node:
		target_position = camera_node.position
		target_z = camera_node.position.z

func _process(delta: float) -> void:
	if camera_node == null:
		return

	var current_pos = camera_node.position
	var new_pos = current_pos.lerp(
		Vector3(target_position.x, target_position.y, current_pos.z),
		smooth_speed * delta
	)
	var new_z = lerp(current_pos.z, target_z, zoom_speed * delta)
	camera_node.position = Vector3(new_pos.x, new_pos.y, new_z)

func _gui_input(event: InputEvent) -> void:
	if camera_node == null:
		return

	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			target_z += zoom_step
		if event.button_index == MOUSE_BUTTON_WHEEL_UP:
			target_z -= zoom_step
		target_z = clamp(target_z, 2.0, 4.0)

		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				is_dragging = true
				drag_start_mouse_pos = event.position
				drag_start_camera_pos = camera_node.position
			else:
				is_dragging = false

	if event is InputEventMouseMotion and is_dragging:
		var from_start = camera_node.project_ray_origin(drag_start_mouse_pos)
		var dir_start = camera_node.project_ray_normal(drag_start_mouse_pos)
		var plane = Plane(Vector3(0, 0, 1), 0)

		var intersection_start = plane.intersects_ray(from_start, dir_start)
		if intersection_start == null:
			return

		var from_current = camera_node.project_ray_origin(event.position)
		var dir_current = camera_node.project_ray_normal(event.position)
		var intersection_current = plane.intersects_ray(from_current, dir_current)
		if intersection_current == null:
			return

		var world_delta = intersection_current - intersection_start

		target_position = Vector3(
			drag_start_camera_pos.x - world_delta.x,
			drag_start_camera_pos.y - world_delta.y,
			target_z
		)
