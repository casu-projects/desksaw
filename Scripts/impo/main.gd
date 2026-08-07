extends Node2D

@onready
var settings = gbData.settings


var screenWidth: int = DisplayServer.screen_get_usable_rect().size.x
var screenHeight: int = DisplayServer.screen_get_usable_rect().size.y

var taskbarPos: int = DisplayServer.screen_get_usable_rect().end.y

var isMacOS: bool = OS.get_name() == "macOS"

@export
var console: Node
# Called when the node enters the scene tree for the first time.
func _ready():
	# macOS: use the full screen rect so the floor reaches the screen bezel (no Dock gap)
	if isMacOS:
		screenWidth = DisplayServer.screen_get_size().x
		screenHeight = DisplayServer.screen_get_size().y
		taskbarPos = screenHeight
		DisplayServer.window_set_size(Vector2i(screenWidth, screenHeight))
		DisplayServer.window_set_position(DisplayServer.screen_get_position())
	else:
		DisplayServer.window_set_size(Vector2i(screenWidth, screenHeight) - Vector2i(1, 1))
		DisplayServer.window_set_position(DisplayServer.screen_get_position())
	
	if OS.get_name() == "Linux":
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_MAXIMIZED)

	if OS.get_name() == "Linux" and OS.get_environment("XDG_SESSION_TYPE").to_lower() == "wayland":
		OS.alert("This message is popping up because you are using wayland. \n \n Most if not all features will not work due to wayland's security measures. \n\n You will have to have another x11 app running to interact with them (ex: Steam) or switch to x11\n\n Sorry! I dont know any workarounds")
	
	GlobalVariable.console.connect(yeah)
	#fix()
	createBorders()

	GlobalVariable.resize.connect(updateBorders)
	pass
	
	var def = gbData.settings.get("defaultSkin", "Body")
	GlobalVariable.userSkinPath = "user://skin/" + def + "/"

	if gbData.settings["expiePersistence"]:
		gbData.temp.expies = gbData.data["save"]["expies"]
		loadExpiePersistence()
	else:
		$CanvasLayer2/ConsoleContainer/Main/ConsoleContainer/Commands.spawnExpie()


func yeah(t: bool):
	console.visible = t
	if t:
		console.showw()


func createBorders():
	taskbarPos = clampi(taskbarPos, 0, screenHeight)
	$Floor.position = Vector2(screenWidth / 2, taskbarPos)
	$SideL.position = Vector2(0, screenHeight / 2)
	$SideR.position = Vector2(screenWidth, screenHeight / 2)


func updateBorders():
	print("resizing")
	var oldheight = screenHeight
	if isMacOS:
		screenWidth = DisplayServer.screen_get_size().x
		screenHeight = DisplayServer.screen_get_size().y
		taskbarPos = screenHeight
		DisplayServer.window_set_size(Vector2i(screenWidth, screenHeight))
		DisplayServer.window_set_position(DisplayServer.screen_get_position())
	else:
		screenWidth = DisplayServer.screen_get_usable_rect().size.x
		screenHeight = DisplayServer.screen_get_usable_rect().size.y
		taskbarPos = DisplayServer.screen_get_usable_rect().end.y
		DisplayServer.window_set_size(Vector2i(screenWidth, screenHeight) - Vector2i(1, 1))
		DisplayServer.window_set_position(DisplayServer.screen_get_position())
	for child in get_tree().current_scene.get_children():
			if child.has_meta("entity") or child.has_meta("object"):
				child.position.y -= screenHeight - oldheight
	createBorders()


func loadExpiePersistence():
	print("loading expies...")
	print("Expie persistence data: " + str(gbData.data["save"]["expies"]))
	
	var x: int = 0
	for names in gbData.data["save"]["expies"].keys():
		x += gbData.data["save"]["expies"][names]
	if x > 20:
		GlobalVariable.persistenceWarning.emit()
		print("Awaiting response from warning popup...")
		await GlobalVariable.persistenceWarning
		print("Response detected. Continuing...")
	
	
	for name in gbData.data["save"]["expies"]:
		print("loading '", name, "' skin expies...")
		for i in range(gbData.data["save"]["expies"][name]):
			await get_tree().create_timer(0.25).timeout
			GlobalVariable.userSkinPath = "user://skin/" + name + "/"
			$CanvasLayer2/ConsoleContainer/Main/ConsoleContainer/Commands.spawnExpie()
			print("loaded ", name, " - ", i)
			gbData.data["save"]["expies"][name] -= 1
