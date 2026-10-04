extends Node2D

@onready var path: Path2D = $Path2D
@onready var line: Line2D = $Path2D/Line2D
@onready var arrow_head: Sprite2D = $Path2D/Line2D/Sprite2D

@export var direction: float = 0.0        # facing angle, radians (0 = +X)
@export var curve_strength: float = 0.6   # how bowed the arc gets
@export var curve_sign: float = 1.0       # flip to -1.0 if the bow is mirrored
@export var smoothness: float = 0.8       # 0 = snap, 1 = very smooth (tip animation)

@export var target: Vector2 = Vector2.ZERO

var _tip: Vector2 = Vector2.ZERO

func _ready() -> void:
	path.curve = Curve2D.new()
	path.curve.add_point(Vector2.ZERO, Vector2.ZERO, Vector2.ZERO)
	path.curve.add_point(Vector2.ZERO, Vector2.ZERO, Vector2.ZERO)

func _process(_delta: float) -> void:
	var mouse := target

	# Animate the tip toward the mouse.
	_tip = _tip.lerp(mouse, 1.0 - smoothness)

	var dist := _tip.length()
	if dist < 0.001:
		line.points = PackedVector2Array()
		return

	var forward := Vector2.RIGHT.rotated(direction)
	var to_tip := _tip / dist

	# sin of the angle between forward and the tip direction.
	# 0 = straight ahead, ±1 = perpendicular. Its sign is the side.
	var sin_a := forward.cross(to_tip)

	var side := signf(sin_a)
	# Behind the arrow → mirror the curve side.
	if forward.dot(to_tip) < 0.0:
		side = -side

	# Bow perpendicular to the straight line, magnitude scales with |sin|.
	var bow_dir := to_tip.orthogonal()
	var bow := bow_dir * side * absf(sin_a) * dist * curve_strength * curve_sign

	# Quadratic control point: midpoint of the chord, pushed sideways.
	var control := _tip * 0.5 + bow

	# Convert the quadratic to cubic handles. Both are RELATIVE offsets.
	var h0 := control * (2.0 / 3.0)              # offset from P0 (origin)
	var h1 := (control - _tip) * (2.0 / 3.0)     # offset from P1 (the tip)

	path.curve.set_point_position(1, _tip)
	path.curve.set_point_out(0, h0)
	path.curve.set_point_in(1, h1)

	var baked := path.curve.get_baked_points()
	line.points = baked

	if baked.size() >= 2:
		var tip: Vector2 = baked[-1]
		var tangent: Vector2 = tip - baked[-2]
		arrow_head.position = tip
		arrow_head.rotation = tangent.angle()
