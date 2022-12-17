// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

GETTING STARTED
===============
Visit https://curvyeditor.com to access documentation, tutorials and references

EXAMPLE SCENES
==============
Checkout the example scenes at "Plugins/ToolBuddy/Assets/Curvy Examples/Scenes"!

NEED FURTHER HELP
=================
Visit our support forum at https://forum.curvyeditor.com

VERSION HISTORY
===============

Note: Starting from version 3.0.0, Curvy is using the versioning standard “Semantic Versioning 2.0.0”. More about it here: https://semver.org/

Warning: At each Curvy update, please delete the old Curvy folders before installing the new version

8.4.0
	General:
		[CHANGED] Made all editors support multi-object editing
		[CHANGED] Enhanced some example scenes
		[NEW] Most console messages now highlight the object that triggered them when clicking on the messages
		[FIXED] Corrected factual error in the documentation of OnBeforeControlPointAdd
	Curvy Generator:
		[CHANGED] Shape Extrusion/Deform Mesh: Better presentation of advanced scaling fields
		[OPTIM] Deform Mesh: better performance when Scale Mode is set to Advanced but its scaling multiplier curves are equivalent to the constant value of 1
		Path Relative Translation:
			[OPTIM] Multithreaded the logic of the module
			[NEW] Made translation that varies along the path possible, by adding the Multiplier setting
			[NEW] Made the translation direction parametrizable through the Angle setting
		Variable Mix Shapes:
			[CHANGED] Invalid Mix Curve values are now substituted with valid ones, in addition to the warning previously shown
			[OPTIM] Updating takes less CPU time
	Line Renderer/ Curvy Spline To Edge Collider 2D:
		[CHANGED] Scripts are now listed under the Converters menu (instead of Misc)
		[CHANGED] The Line Renderer/Edge Collider 2D is now kept as is when the source Spline is set to None (instead of emptied)
		[FIXED] The scripts are being Updated unnecessarily under some circumstances
	Spline Controller:	
		[FIXED] On Position Reached events: In closed loops, events at spline start/end are not always triggered
		[FIXED] On Position Reached events: In closed loops, events not at spline start/end are triggered when the controller goes through the spline's start/end
	API:
		[API/NEW] Added CurvySplineSegment[] Add(int controlPointsCount)
		[API/CHANGED] Method TypeExt.GetLoadedTypes, that was set as Obsolete in 8.1.0, is no more obsolete
		
8.3.0
	[NEW] Deform Mesh module: added a Scale tab with scale parameters, similar to the one in the Shape Extrusion module
	[NEW] Controllers: Added a Constraints setting, to set position and rotation constraints
	[NEW] Controllers: Added a Target Component setting, to define what component the controller should control: Transform, Rigidbody or Rigidbody2D
	[CHANGE] Updated example scene 10_RBSplineController to showcase physics interactions with controllers
	[CHANGE] Create Mesh module: Changed the default value of Group Meshes to false
	[FIXED] In an HDRP build, using a Create Mesh module with the Combine setting set to true generates errors
	[FIXED] Rasterize Path module: Setting the Range to a zero length range resets all the settings of the module
	[FIXED] Prefab Pool: error when creating a new pool with no name
	[FIXED] Prefab Pool: when name is empty, adding a prefab does not automatically set the name of the pool
	[API/CHANGE] The default value of PrefabPool.Identifier is now String.Empty instead of null
	[API/FIXED] Typo in BuildVolumeMesh.MaterialSetttings

8.2.2
	[FIXED] Selecting a spline hides its gizmo when using Unity 2022.1

8.2.1
	[FIXED] Console error when showing a Spline Controller's inspector under specific Unity versions

8.2.0
	[NEW] Added warnings in InputMesh and GameObjectToMesh modules when meshes are not readable
	[FIXED] Deform Mesh module: Errors when used with a mesh with no tangents
	[FIXED] Deform Mesh module: runtime errors in builds lacking the Diffuse material
	[FIXED] IL2CPP builds: Error when unloading/switching scenes containing Array Pools
	[FIXED] IL2CPP builds: runtime error
	[FIXED] Example scenes: Error message in scene 51, and setup problem in scene 50
	[API/NEW] Spline input modules: Added SetRange and ClearRange methods

8.1.0
	[NEW] Curvy Spline Segment: added the CP's Relative Distance to the info box
	[NEW] Create Mesh module: added a 3rd export button, to export meshes to both the scene and assets.
		This is useful when you want to make a prefab out of the exported to scene gameobject
	[CHANGED] Made prefabs containing scripts from Curvy Splines openable in Play Mode
	[CHANGED] Position gizmo for Bezier handles is now slightly different than Unity's position gizmo
		This is to make it easier to distinguish the position gizmo of a Control Point from its handles'
	[CHANGED] Volume Spots module: the minimal allowed depth for objects was changed from 0.01 to 0.001
		This is to allow for placement of smaller objects
	[OPTIM] Reduced various initialization times, as well as entering Play Mode and reload times
		This is especially visible in projects with a big number of scripts
	[FIXED] Building for iOS platform leads to errors
	[FIXED] Under Unity 2022.1 beta, an error message is sometimes displayed at project reloading 
	
8.0.2
	[OPTIM] Minor optimization of the Deform Mesh module
	[FIXED] Control point's gizmo labels are not positioned properly under certain Unity versions
		This bug is due to a regression in Unity that has been fixed lately. Curvy had a workaround to that regression. This update disables the workaround for the latest Unity versions that don't need the workaround. It is recommended to use both Curvy 8.0.2 or higher, and Unity 2021.2.12f1 or higher.
		More about the issue here: https://issuetracker.unity3d.com/issues/handles-dot-label-does-not-appear-in-the-supposed-place
	
8.0.1
	[NEW] Added a warning when a spline's gizmo color is transparent
	[FIXED] Spline's gizmo colors are sometimes automatically set to a transparent color
	[CHANGED] Create Mesh module: GroupMeshes is now hidden if Combine is set to false
		Before this change, the GroupMeshes parameter was visible even when its value was not used
		
8.0.0
	TL;DR Deform Mesh and other new CG modules, Spline Controller's On Position Reached events, SVG import, B-Splines, Spline To Edge Collider 2D, major optimizations, a LOT of fixes and an easier workflow

	General
		[NEW] Added a Curvy Spline To Edge Collider 2D script
		[NEW] Added an Array Pools Settings script
			Curvy Splines now use pools of arrays as a mean to reduce CPU time. These pools have a default capacity. Automatically, an instance of Array Pools Settings is added to the _CurvyGlobal_ object. It allows you to define the capacity of the pools, and shows you their current usage. Set the Capacity to 0 to not use the pools at all
		[CHANGED] Changed the folders structure. Now everything is in a Plugins/ToolBuddy folder. As recommended for every update, please delete the old Curvy folders before installing the new version
		[CHANGED] Improved various UIs, including the Curvy Generator: improved ease of use, added tooltips, added warning messages when relevant, etc...
		[CHANGED] Various documentation enhancements
		[OPTIM] Optimized various features, both in edit mode and play mode. The optimizations are related to both CPU time and memory usage. The biggest of these optimizations greatly improves scenarios involving frequent updates of Curvy Generator, such as real-time mesh generation
		[FIXED] Toolbar not displaying when setting the Unity Editor language to a language other than English
		[FIXED] Truncated text in some buttons due to new UI
		[API/CHANGED] SerializableArray has changed namespace from FluffyUnderware.CurvyEditor to FluffyUnderware.Curvy.Utils

	Splines
		General
			[NEW] Support for B-Splines
			[NEW] Restrict To 2D can now restrict the spline in any of the main 3 planes. You can select the plane via a new setting, Restricted 2D Plane
			[NEW] Nearest Spline Point: a script to get the nearest point on a spline
			[CHANGED] Splines created via the editor have now a numbered suffix added to their name
			[CHANGED] When adding a Control Point to a spline, its local rotation is set to zero, instead of its global rotation as it was the case
			[CHANGED] Spline segments have now a maximal cache size of 1.000.000 points
			[FIXED] Curvy Spline inspector showing a false warning about a spline not being restricted to a 2D plane
			[FIXED] Sometimes splines refresh every frame even when there are no changes to process
			[FIXED] "Set center/pivot point" tool in the toolbar does not update when a spline switches from being 3D to restricted to 2D
			[API/NEW] Added CurvySpline.GetNearestPoint
			[API/NEW] Added CurvyUtility.SplineToMesh
			[API/NEW] Added CurvyUtility.GetNearestPointIndex
			[API/NEW] CurvySpline.Clear has now an optional parameter to define if the spline clearing should be undoable in the editor or not
			[API/NEW] Added new methods to get the spline's cached data. Those methods use pooled arrays: GetPositionsCache, GetTangentsCache and GetNormalsCache
			[API/CHANGED] Start and OnEnable now invoke the OnRefresh event always if the spline has Control Points, and never if the spline is empty
			[API/CHANGED] Refresh now invokes the OnRefresh event only if there were actual modifications processed by the Refresh method
		Scene view UI
			[NEW] Added an Orientation Anchors gizmo
			[NEW] Added a Relative Distances gizmo
			[NEW] Added a TF gizmo
			[NEW] Draw Splines tool: Added a new drawing plane, the view plane
			[NEW] Draw Splines tool: CTRL+clicking on the first Control Point of a spline closes it/opens it
			[CHANGED] Connection gizmos are now a separate setting from the Curve gizmo. They can now be activated separately
			[CHANGED] Control Points' Label gizmos are now displayed even when the CP is not selected
		Shapes
			[CHANGED] Pie's roundness is now by default 1 when shape is reset
			[FIXED] Shapes are not refreshed when their component are reset via the inspector
			[FIXED] Pie's normals are incorrect
			[FIXED] Rectangle's normals are incorrect when increasing the cache density
			[FIXED] Shapes not having the proper normals when setting planes other than the default one
		Splines Import/Export window
			[NEW] Added support for SVG import
			[CHANGED] The local/global selector now applies also on the spline's coordinates, not only the Control Points'
			[CHANGED] File selector now displays only the relevant file formats
			[FIXED] Synchronize TCB is ignored when importing/exporting a spline
			[FIXED] Error when exiting the file selector without choosing a file
			[API/NEW] Exposed methods converting between splines and JSON in SplineJsonConverter

	Controllers
		[NEW] Spline Controller: Added new type of events, "On Position Reached" events, allowing for events to be triggered at any position on the spline
		[CHANGED] Position Mode's default value changed from Relative to Absolute
		[CHANGED] When Move Mode is set to Absolute, Position slider is now greyed when no source for the controller is set
		[FIXED] Errors when UITextSplineController has spaces in its input text
		[FIXED] Spline Controller: Wrong documentation link for the Connections Handling section
		[FIXED] Rarely, an exception is thrown when using a spline controller
		[FIXED/API] SplineController.SwitchTo leads to errors when called with a duration value of 0

	Curvy Generator
		General
			[NEW] New module: Deform Mesh. Deforms a given mesh along a path
			[NEW] New module: Input Transform Spots. Sets a spot that is tied with a Game Object's transform
			[NEW] New module: GameObject To Mesh. Converts GameObjects to Volume Meshes
			[NEW] Create GameObject module: Added a new setting: "Apply Layer On Children"
			[CHANGED] Create Mesh module: a 32 bit index format is now used if the mesh has too many vertices, allowing to create meshes with more than 65535 vertices
			[CHANGED] Create Mesh module: The default mesh collider's cooking options now includes Use Fast Midphase for the relevant Unity versions
			[CHANGED] Create Mesh module: The AddNormals and AddTangents enums are replaced with boolean parameters
			[CHANGED] Volume Mesh module: Removed Material ID from the UI since it was not used. The used material id is the material group's index (displayed as the tab's title)
			[CHANGED] Input Spline Shape module: created shapes no more have their gizmos hidden by default
			[CHANGED] Curvy Generators' instances now have incremental names
			[CHANGED] Shape Extrusion Module: the following settings are not available anymore, and are assumed to be always true: Hard Edges, Materials and Extended UV
				The reason behind this change is to lift the legitimate confusion some users had: until now, to use one of those settings in a shape extrusion, you had to both activate the setting in the module and activate the equivalent setting in the "Meta CG Options" meta data of the relevant Control Point. Now you only need to activate the setting in the meta data
			[OPTIM] Create Mesh module: When creating multiple meshes with mesh colliders, the colliders are baked in parallel
				Warning: as a limitation from Unity, the parallel baking will be ignored if you set the cooking options to any value different than the default value. For more details: https://forum.unity.com/threads/physics-bakemesh-question.741257/#post-4999400
			[FIXED] The Auto Refresh feature stops working properly if you switch scenes without destroying the Curvy Generator
			[FIXED] CG's ordering of its modules in the hierarchy sometimes is not consistent
			[FIXED] Create Mesh module: When setting Combine to true, an exception is thrown when using spots with a mesh that has no materials
			[FIXED] Create Mesh module: An exception is thrown when inputting a spot with an invalid index (referencing a non existing VMesh input)
			[FIXED] Create Mesh module: when combining two meshes, one with tangent and one without, the Add Tangents parameter is ignored
			[FIXED] Create Mesh module: when AddNormals and AddTangents are set to No, the normals and tangents are still included in mesh, and have invalid values
			[FIXED] Create GameObject and Create Mesh modules: Resources counter and selector sometimes do not work properly
			[FIXED] Volume Spots modules created via API can under some circumstances get their value reset
			[FIXED] Volume Mesh module: Material settings are not undoable
			[FIXED] In Play Mode, deleting the input splines of the Input Spline modules, if done through the module's UI, lead to incomplete deletion of said splines
			[FIXED] Input Spline modules: Sometimes an error is logged when rasterizing a spline too densely
			[FIXED] Input Spots module: trying to add an input after deleting all existing ones generates errors
			[FIXED] Input Spots module: Errors when dragging a Game Object over the module
			[Fixed] Input Game Objects: when an entry has no game object assigned, the module outputs a null game object instead of just ignoring it
			[FIXED] Rasterize Path module: The Range and Length parameters behave as if an open spline is closed, and vice versa
				To restore the previous behaviour if your project depends on it, set the Use Bugged Range setting to true
			[FIXED] Shape Extrusion module sometimes extrudes a faulty mesh when it uses a different range than 0-1 and the associated Input Spline module uses a Start CP different than the first CP
			[FIXED] Shape Extrusion module: When path range loops, and when using the Optimized setting, Advanced Scaling does not work on the path part that is beyond the looping point
			[FIXED] Input Mesh module showing a mesh as having a UV2 channel when it hasn't
			[FIXED] Variable Mix Shapes module sometimes not updating when the associated path changes slightly
			[FIXED] TRS Path module: If applied on a closed path, any extruded mesh using said path is not aligned at its looping point
			[FIXED] Editor lags when refreshing a generator in projects that have a lot of assets
			[FIXED] Dropping a spline in the generator window sometimes does not create an input module
			[API/NEW] Added CGVolume.Scales
			[API/NEW] CGVMesh.GetSortedVertexIndices
			[API/CHANGED] CGVMesh.ToMesh now has two optional parameters defining whether the normals and tangents should be copied or not
			[API/CHANGED] Made mandatory the previously optional includeOnRequestProcessing parameter of the CurvyGenerator.GetModules methods. It being optional and having a default value set to false made it confusing for its users
			[API/CHANGED] The BuildRasterizedPath.From setter now handles the edge value of 1 similarly to BuildShapeExtrusion.From
			[API/FIXED] The following methods ignore the modules that implement the IOnRequestProcessing interface: CurvyGenerator.GetModuleOutputSlot(int, string) and CurvyGenerator.GetModuleOutputSlot(string, string)
			[API/FIXED] Conform Path and Path Relative Translation modules: the output path's Length and F are not updated
			[API/FIXED] The CGSpots(CGSpots source) constructor does not copy the source spots, but copies just the reference
			[API/FIXED] Pushing then popping an instance of CGMeshResource in the same frame can lead to some of its components to be deleted
		Exporting Game Objects/Meshes
			[NEW] Added a Save Output To Scene button in the toolbar. This will copy to outside the generator all the generated meshes and game objects
			[NEW] Added a Clear Outputs button in the toolbar. This will delete all the generated meshes and game objects in a single click
			[NEW] Create GameObject module: Added a Save To Scene button
			[CHANGED] Create GameObject and Create Mesh modules: Now exported game objects are always under a parent game object, even if there is only one exported game object
			[CHANGED] Create GameObject and Create Mesh modules: When multiple objects are exported, their parent GameObject is now named "<module_name> Exported Resources" instead of "<module_name>"
		Prefabs
			[CHANGED] Prefabs: Better handling of forbidden prefab related operations
				Some operations, like deleting a game object, are not allowed in a prefab instance due to Unity's New Prefab System. When Curvy Generator was asked to delete an object part of a prefab instance, it showed an error message in the console. Now, it will behave as follows:
					- If the object to delete is automatically generated, like an output mesh, and thus can be regenerated by refreshing the Curvy Generator, any operation that leads to its automatic deletion will also delete it from the prefab asset
					- For the other cases, like deleting a module from a graph window, a popup will ask the user to first delete the associated object from the prefab asset
			[FIXED] Create GameObject module: Errors when using input GameObjects that are prefab assets
			[FIXED] Create GameObject and Create Mesh modules: Objects are sometimes duplicated when exiting play mode if the module is part of a prefab
		Graph View
			[NEW] Support for the following hotkeys for modules manipulation:
				Ctrl+D duplicate selected module
				Ctrl+A select all modules
				Ctrl+C copy selected module
				Ctrl+X cut selected module (module is deleted when pasting the cut module)
				Ctrl+V past module
			[NEW] Support for drag and drop of Game Objects, shapes and meshes in the generator view
			[CHANGED] Dropping a link on a module no more selects and focuses the view on said module
			[CHANGED] Module focus is now on the top of the module instead of its bottom. This makes working with big modules, such as Volume Spots, easier
			[CHANGED] A right click on a module now also selects it
			[CHANGED] Dragging a module now selects it
			[FIXED] CG's graph sometimes scrolls in weird directions when dragging a module
			[FIXED] Couldn't deselect a selected module under some circumstances 
			[FIXED] Couldn't do a rectangle drag under some circumstances
			[FIXED] When clicking on a module's title to drag the module, sometimes a link is dragged instead
			[FIXED] CG's graph keeps scrolling by itself
			[FIXED] CG's graph's preview link keeps being displayed even after the user stopped dragging it
			[FIXED] When a selected module is deleted from outside the generator, the latter keeps highlighting the module's contour as if still existing
			[FIXED] Clicking on an empty space does not deselect the selected link
			[FIXED] When a link is under a module, selecting the module can select the link, and sometimes delinking it
			[FIXED] Scrolling bars glitch when trying to drag the view beyond the graph's borders
			[FIXED] Selecting modules via selection rectangle does not select the module's Game Object
			[FIXED] Finishing canvas dragging while the mouse is on a module selects the module
			[FIXED] When deselecting a module by clicking on an empty spot on the graph, the deselection takes a delay to be applied if the mouse is not moving
			[FIXED] Canvas drag stops when hitting any keyboard key
			[API/FIXED] CanvasState.MouseOverLink is giving wrong results
		UV and UV2
			General
				[NEW] Create Mesh module: When combining multiple Volume Meshes, you now have the option to recompute UV2 for the combined mesh. This can give better results than computing UV2 for each Volume Mesh individually. That option is called Unwrap UV2
				[NEW] Volume Mesh module: Added a new setting, Unscale U, that allows for the U coordinates to be independent from the scaling
				[CHANGED] Create Mesh module: Replaced "Add UV2" with Volume Mesh module's "Generate UV2". Now both UV and UV2 are generated by the Volume Mesh module
				[CHANGED] Volume Mesh module: the Keep Aspect setting now has three possible values: Off, Scale U or Scale V
				[FIXED] Volume Mesh module: UV2 calculation is wrong when using the Split setting
					For backward compatibility reasons, the pre Curvy 8 behaviour is still available through the Split UV2 setting, in the Backward Compatibility section
				[FIXED] Volume Mesh module: UV2 incorrectly computed when UV is swapped or negatively scaled
				[FIXED] Volume Mesh module: Tangents are invalid when UVs are missing, swapped or scaled negatively
				[FIXED] Spline To Mesh window: Wrong UV2 values
				[FIXED] Build Volume Caps module: Wrong UV2 values
				[FIXED] Volume Caps module: The Keep Aspect enum does not work properly
			Meta CG Options
				[CHANGED] Now shows only the settings that are relevant (that will be used in that specific context)
				[CHANGED] "Explicit U" and "Set U from neighbours" are now possible for CPs with a change of Material ID
				[FIXED] When there is a change of Material ID at a specific CP, its "UV Edge" is overridden to true
					For those relaying on that old behaviour, you will have to set "UV Edge" to true for all the CPs with a change of Material ID. You can find a code snippet that does that here: https://forum.curvyeditor.com/thread-1294.html
				[FIXED] On the first CP of an open spline, in some cases First U is ignored when setting Explicit U (second U is used instead)
				[FIXED] On the first CP of an open spline, First U and second U can be displayed even if the CP is not a UV Edge or has Explicit U
				[FIXED] Exception is sometimes thrown when last CP of a spline has an Explicit U value
				[FIXED] ArgumentOutOfRangeException when computing UV under some circumstances
				[FIXED] When setting two points CPs to the same explicit U, the second CP will have an invalid U value under some circumstances
				[FIXED] When setting an explicit U value to any CP other than the last one of an open spline, the automatically generated U value of the last CP is wrong
				[FIXED] Invalid UV coordinates in some cases when using two different materials on closed splines
				[FIXED] Shape Extrusion module: UV coordinates of an extruded shape are wrong if you activate Hard Edges and Extended UV options while not providing any value for those options
				[API/CHANGED] MetaCGOptions.HasDifferentMaterial does not create anymore a MetaCGOptions instance at the previous CP if such instance does not exist
		Data disposal (API only)
			In connection with the array pools addition explained earlier, instances of CGData and their inheriting classes need to be disposed. This allows for the arrays they use to be returned to the pools
				[API/NEW] Added OutputSlotInfo.DisposableData: Whether the data outputted through this slot can be disposed when it is replaced by a newer outputted data
				[API/NEW] Added an override for the GetData methods from the  CGModuleInputSlot. That override has an out boolean parameter, isDataDisposable. That parameter tell the data consumer if the data should be disposed after being used or not

	Preferences window
		[NEW] New setting: Save Generator Outputs
		[NEW] New setting: Show Hints
		[NEW] New setting: Enable Announcements
		[NEW] New setting: Enable Metrics
		[NEW] New setting: Auto Fade Labels
		[CHANGED] When resetting preferences, Use Tiny 2D Handles is now set to true
		[CHANGED] Changed the default value of Spline Selection Color to a color that is more visible on dark backgrounds. To set back the previous color, please set the color's RGB 0-1 values to red:0.15, green:0.35 and blue:0.68
		[FIXED] Resetting preferences does not reset the "Hide _CurvyGlobal_" and "Snap Value Precision" preferences

7.1.8
	[FIXED] The URL of the Upgrade button is leading to an obsolete package

7.1.7
	[NEW] Added an Upgrade to Curvy 8.0.0 button, which leads to https://assetstore.unity.com/packages/slug/212532
		Make sure you are logged in the Asset Store when accessing that page
		
7.1.6
	[FIXED] Curvy shows a compiler warning when used with Unity 2021.2

7.1.5
	Curvy Generator
		[CHANGE] Now the order of execution of CG modules is deterministic
		[FIXED] CG's ordering of its modules in the hierarchy sometimes is not consistent
		[FIXED] Input Mesh module showing a mesh as having a UV2 channel when it hasn't
	Controllers
		[FIXED] Errors when UITextSplineController has spaces in its input text
		[FIXED] Wrong documentation link for the Connections Handling section in the Spline Controller's inspector
		[FIXED/API] SplineController.SwitchTo leads to errors when called with a duration value of 0
	Splines
		[FIXED] Sometimes splines refresh every frame even when there are no changes to process
		[FIXED] Inspector showing a warning when it shouldn't, about a spline not being restricted to a 2D plane
	Others
		[FIXED] Toolbar not displaying when setting the Unity Editor language to a language other than English
		[FIXED] Text is truncated in some buttons
		[FIXED] Rectangle's normals are incorrect when increasing the cache density
		[FIXED] "Set center/pivot point" tool in the toolbar does not update when a spline switches from being a 3D to one restricted to 2D

7.1.4
	Curvy Generator
		[CHANGE] Spline Input modules: Now when the StartCP or EndCP is automatically set to null because its value is invalid, an error message is displayed in the console to notify the user about the value override and the reason behind it
		[OPTIM] TRS modules and the Relative Translation module: less CPU consumption
		[FIXED] Shape Extrusion and Path Rasterization modules: When the Range parameter loops, and if using the Optimized option, the Advanced Scaling does not work on the path's part that is beyond the looping point
		[FIXED] TRS modules: Using a TRS module on a closed path makes the extruded mesh using it not align at its looping point
	Shapes
		[FIXED] Sometimes, after selecting a spline, the Shape creation and edition buttons need to be clicked twice to work 
		[FIXED] The Shape Wizard's background is not drawn properly when the Curvy toolbar is placed at the bottom side or right side of the screen
		[FIXED] Creating a Shape through the Shape Wizard when having another shape selected leads to creating an empty shape
		[API/CHANGE] Made CurvyShape.Persistent obsolete
	Others
		[NEW] Added the Asteroid Belt Youtube tutorial scene as example scene 28
		[CHANGE] Replaced example scene 24 with the Conform To Terrain Youtube tutorial scene
		[CHANGE] Added a background to the Shift tool's UI for more visibility
		[CHANGE] Replace a Connection Synchronization icon for the dark theme UI for more visibility
		[CHANGE] Minor improvements in UI and documentation
		[FIXED] A compiler warning appears when using Unity 2020.2 or above
			Sorry about this one, the fix was accidentally not included in 7.1.3.

7.1.3
	[FIXED] Compiler warnings appear when using Unity 2020.2 or above

7.1.2
	[FIXED] Curvy Generator: module's background does not have the right size on Unity 2019.3 and newer
	
7.1.1
	[FIXED] Compiler error when using Unity 2019.1
	
7.1.0
	Unity editor's Undo
		[CHANGE] Made the Make Planar operation undoable
		[CHANGE] Made the Connection inspector's operations undoable
		[FIXED] Undoing Bézier handles modification does not reflect on connected control points when Bézier handles are set to be synchronized
		[FIXED] Undoing the creation of control points via the "Add & Smart Connect" tool (CTRL + right click when creating splines) sometimes is not done properly or even leads to errors
		[FIXED] Undoing the removal of a connected control point does not restore the connection
		[FIXED] Undoing the removal of a connected control point stops its spline from refreshing when said control point is moved
		[FIXED] When undoing shape modifications done via the shape wizard, the shape can become weird looking
		[FIXED] Deleting a Control Point, if undone, will restore the CP at the wrong position in the spline's hierarchy
		[API/NEW] Added CurvySpline.Delete(CurvySplineSegment controlPoint, bool skipRefreshingAndEvents, bool isUndoableDelete)
	Connections
		[CHANGE] "Add and smart connect" tool: Now all the added CPs have their heading set to automatic
		[FIXED] "Add and smart connect" tool: Adding a CP to an existing connection, having some CPs synchronized and others not, can lead to all the connection's CPs to synchronize their transform
		[FIXED] A console error is displayed when the Simplify tool removes a connected control point
		[FIXED] When rotating a Bézier spline's control point while holding the Shift button, the rotated handles synchronize with the connected control points even if the handles synchronization option is not activated
		[FIXED] In some situations, Follow-Up's value is not taken into consideration if the connection's inspector is not displayed
		[FIXED] Follow-Up: Automatic heading direction is sometimes not resolved properly.
			The "Automatic" heading direction will now be resolved to "To spline's end" instead of "Nowhere" when no obvious value is found. It gives better result most of the time
		[FIXED] Duplicating a connected spline will create a spline that seems connected while it isn't
		[API/NEW] Added CurvyConnection.Disconnect(bool destroyEmptyConnection)
		[API/NEW] Added CurvySplineSegment's CanFollowUpHeadToEnd and CanFollowUpHeadToStart
		[API/CHANGE] Made CurvyConnection.ResetConnectionRelatedData obsolete
		[API/FIXED] CurvyConnection.RemoveControlPoint does not unset the removed CP as a Follow-Up for the still connected CPs
	Others
		[NEW] Added a button in the about window to display the publisher's page on the asset store
		[NEW] The following metrics are now send to Curvy's server at each import of a new Curvy version: Curvy version, Unity version, Scripting version.
			The collected metrics will help making decisions when it comes to maintaining Curvy's old code, and using Unity's version specific features. The send data is anonymous
		[CHANGE] Made the "Generate Assembly Definition" button's icon compatible with the dark ("Professional") editor theme
		[FIXED] Wrong UV2 calculation when UV scale is in between 0 and 1
		[FIXED] When Restrict To 2D is true, and the spline is not planar, the Make Planar button is sometimes not displayed
		[FIXED] Errors when the project uses the Code Analysis package
		[FIXED] Runtime instantiated controller throws null reference exceptions when one of its events is called
		[FIXED] Error messages sometimes showing in the example scenes using trains
		[FIXED] When resolution is too low, mesh extrusion sometimes creates an unnecessary point at the end of a spline when rasterizing it
		[API/CHANGE] CurvyMetadataBase class now requires a CurvySplineSegment component, to avoid misuse
		[API/CHANGE] CurvySpline: Equalize, MakePlanar and Normalize now Refresh the spline after modifying it. This is done to harmonize the behaviour with other similar methods
		[API/FIXED] CurvySpline.ToLocalDirection does not return the expected result
		[API/FIXED] CurvySpline.IsControlPointASegment returns true for all PCs of a closed spline, even if said spline has only one CP
			This bug lead to erroneous values for the following properties: FirstVisibleControlPoint, FirstSegment, LastVisibleControlPoint and LastSegment
		[API/FIXED] CurvySpline's GetNextControlPointIndex and GetPreviousControlPointIndex returning 0 instead of -1 when called on a CP that is the only CP of a closed spline

7.0.0
	[FIXED] Added CPs through the Add & Smart Connect tool are not all undoable
	[FIXED] Creating a CP through the Add & Smart Connect tool sometimes leads to a misplaced CP
	[API/CHANGE] Removed obsolete methods
	[API/CHANGE] Added a space parameter to define in which space a vector is (local or global) for all CurvySpline's and CurvySplineSegment's methods that have a vector as an input or output
	
6.4.1
	[FIXED] The Spot GameObjects template sets Items # to an invalid value of -1
	
6.4.0
	Curvy Generator
		Volume Spots
			[NEW] You can now set the scale of the spots
			[NEW] You can now set the translation in all directions, and in either the relative or absolute frame
			[CHANGED] Modified the module's inspector to make it easier to use and understand. Some parameters changed name in the process
			[CHANGED] Use Volume is now set to true by default
			[CHANGED] Use Volume is no more automatically set to false when input is not a volume (in which case its value is ignored) or the module is reset
			[FIXED] When using Rotation Scatter, the rotation value changes depending on whether Height is set to a fixed value or a random value
			[FIXED] Rotation Scatter is not applied on all objects
			[FIXED] Changing the Seed parameter has no effect on the randomly generated values
			[NEW] Added a Use Bugged RNG option in the General tab. Use it to ignore the fixes listed above if your project depends on the bugged behaviour
			[FIXED] When Space Before or Space After has a too small negative values (i.e too big absolute value), Unity freezes
			[FIXED] Setting the Space Before, Space After, or Position Offset as random values will modify the randomly generated rotations and height
			[FIXED] A group's Space Before and Space After are considered even if the group is not placed because of Keep Together
			[FIXED] When a repeating group with Keep Together set to true is not placed because of a lack of space, the following group is positioned as if the first group was placed
			[FIXED] When a group has Keep Together set to false, and there is no room to fit all group's items, the placed items are out of order
			[FIXED] When a group, that is not part of the repeatable groups, has items that are not randomly selected, those items are placed at the wrong position
			[Fixed] When in a group some items are randomly selected, some items are placed too far from where they should be
			[FIXED] Console shows errors when a Volume Spots has no group in it			
		Shape Extrusion
			[FIXED] Invalid mesh produced if resolution is too low
		Create Mesh
			[NEW] Added Capsule as an option for collider types
			[NEW] Exposed Rigidbody.IsTrigger
			[FIX] Save to scene creates object at the wrong position when generator is not a position (0,0,0)
		Others
			[CHANGE] UI tweaks in Volume Spots, Shape Extrusion, Input GameObjects and Input Mesh modules
			[CHANGE] Updated the built-in templates:
				Shape Extrusion now have an input spline already assigned
				Spot Templates now have an input volume already linked, and Use Volume set to true
			[API/FIX] CGModuleInputSlot.LinkTo allows to link multiple outputs to the same input even if said input accepts only one link
	Curvy Spline		
		[OPTIM] Splines take less CPU time in their per frame checks
		[OPTIM] Spline's interpolation and update were further optimized. This will impact most spline's operations
		[FIXED] Computation of TCB and CatmullRom splines when having a follow up was wrong
		[FIXED] In some rare cases, the gizmo is displayed one frame late
			A consequence of this fix is that the OnDrawGizmosSelected and OnDrawGizmos methods can call the spline's Refresh method. Keep this in mind when profiling
		[API/NEW] Added GetFollowUpHeadingControlPoint
		[API/CHANGE] Calling SetFollowUp with an invalid parameter will now show an error message
		[API/CHANGE] CalculateSamplingPointsPerUnit now always return non zero values
	Others
		[NEW] Added a Generate Assembly Definitions button in the toolbar, under Curvy Options
			This solution replaces the previously included *.asmdef.disabled files.
		[NEW] Added a Review button and a Custom Development button to the About window
		[CHANGE] Curvy Line Renderer: Added warning when Spline is not assigned
		[CHANGE] Removed from the components list the ones that are not meant to be instantiated directly
		[FIXED] Some component's help button do not lead to a valid documentation page
		[API/CHANGE] CurvyConnection.AddControlPoints now shows an error message when it ignores a Control Point that has already a connection
		[API/CHANGE] Set components that are never directly instantiated by Curvy as abstract classes
		[API/CHANGE] Calling a teleport method on a stopped controller displays an error message

6.3.1
	[FIXED] Compilation failure when using the provided asmdef files

6.3.0
	Controllers
		[OPTIM] Optimized Spline Controller
		[FIXED] Path and Volume controllers are stuck at spline's end when using PingPong clamping while Move Mode is set to Relative
		[FIXED] Controller can't be positioned at the end of an open spline when Clamping is equal to Loop
		[FIXED] In some situations, controllers get stuck at Control Points
	Others
		[OPTIM] Optimized some low level Curvy Spline methods
		[CHANGE] Inspector of the Curvy Spline Segment now shows the TF too
		[CHANGE] Reduced size of example assets, and tweaked some example scenes
		[API/NEW] New overrides of TfToSegment and DistanceToSegment

6.2.0
	Curvy Generator
		[FIXED] CreateGameObject: When entering play mode, The GameObject's scale seems to be multiplied by the scale value twice
		[FIXED] BuildVolumeSpots: The rotation of the input GameObject influences the number and positions of the spots when it shouldn't
		[FIXED] Mesh error when using a RoundedShape in an extrusion with a Roundness of 0
		[FIXED] Error messages about dirty modules displayed when it should not
		[FIXED] Errors when creating a generator in an unsaved scene and then creating a new scene
			A consequence of this fix is that a Curvy Generator component will move up in the list of its related gameobject's components
		[FIXED/CHANGED] Various UI fixes and tweaks, most of them for all Unity versions, few of them exclusive to the new Unity's UI (2019.3 and above)
	Others
		[OPTIM] Various optimizations having impact on the following:
			- Spline gizmo display
			- Spline Controller
			- Spline update and data fetch
			- The Create GameObject CG module
		[FIXED] By default, the update method of scripts is called less frequently in Edit Mode. Having a Controller or a Curvy Generator in a scene and VSync enabled on 2017.2 and beyond stops that behaviour, which increases power consumption
			For people wanting to keep the previous behaviour, a "Force Frequent Updates" toggle was added in the "Advanced Settings" of those two scripts
		[FIXED] If a connected spline is disabled, and if the user switches scene ingame, the connection object throws an error
		[API] Marked some Curvy Spline methods as obsolete

6.1.0
	Curvy Generator
		[FIXED] Generators break when having other generators as children
		[FIXED] The Auto Refresh parameters where ignored when modifying an input spline that is not a child of an input module.
		[FIXED] Volume Caps module: end cap not using the right volume scaling
	Others
		[FIXED] Controllers: Offset feature still applies even when it should not i.e. when the Orientation mode is set to None.
		[FIXED] The UI is broken in some places when using new UI system introduced by Unity 2019.3
		[FIXED] When using Unity 2019.3, you can't assign a dynamic event handler to events inherited from parent class
		[FIXED] When using Unity 2019.3, warnings are displayed when opening some example scenes
		[FIXED] In rare cases, singletons can be duplicated
		[API/CHANGE] Controllers have now some previously private members exposed to inheriting classes. This is to make it easier to make custom controllers.

6.0.1
	[OPTIM] Reduced cost of drawing a spline's gizmos
	[FIXED] The Create GameObject CG module, when instantiating a prefab, breaks the link between the instantiated objects and their prefab

6.0.0
	Curvy Generator
		[NEW] Variable Mix Shapes module: Interpolates between two shapes in a way that varies along the shape extrusion. This module is used in the example scene 27.
		[OPTIM] Modules that are out of the view are now culled
		[OPTIM] Optimized shapes interpolation
		[CHANGE] Shape Extrusion module: The shape used in the extrusion can now vary along the extrusion. To do so, you should link as a shape input a module that provides a varying shape. This is the case of the Variable Mix Shapes module.
		[CHANGE] Made TRS modules larger to show more digits
		[CHANGE] Better error messages when something goes wrong with resources creation
		[FIXED] Mix Shapes module: Normals interpolation in not correctly computed
		[FIXED] Null reference exceptions happen when having as inputs splines with no Control Points
		[FIXED] Mix Shapes and Mix Paths modules: The mixing slider did not considered the mixed paths/shapes in the same order consistently. The order was sometimes inverted depending on the complexity of said paths/shapes
		[FIXED] When a scene has input spline path or input spline shape module, renaming objects from the hierarchy or though the F2 shortcut does not work
		[FIXED] Null references when feeding the output path of BuildRasterizedPath or the output volume of BuildShapeExtrusion to the input of a module that takes a CGPath as an input
		[FIXED] Volume Spots module: Null reference exception when using random distribution on items all of them of weight 0
		[FIXED] Create Mesh module: Null reference exception when unloading a scene having a Create Mesh module which mesh was removed by hand through the inspector
		[API/NEW] New types used by shape extrusions with variable shapes: CGDataRequestShapeRasterization, ShapeOutputSlotInfo and SlotInfo.SlotArrayType
		[API/CHANGE] ModifierMixShapes.InterpolateShape has now an optional ignoreWarnings parameter
	Control Point's Metadata
		[CHANGE] Displays warnings when a Control Point has multiple instances of the same Metadata type
		[FIXED] MetaData gets duplicated
		[API/CHANGE] Started refactoring Metadata API. Obsolete MetaData API will be removed in version 7.0.0. More details here: https://forum.curvyeditor.com/thread-706.html
	Others
		[OPTIM] Various minor optimizations
		[CHANGE] Corrected scene 24's name
		[FIXED] CurvyUISpline's documentation link leads to the wrong page
		[FIXED] UITextSplineController have wrong position when its canvas is scaled
		[FIXED] CurvyLineRenderer not updating when attached to an object that is not a CurvSpline
		[FIXED] CurvyLineRenderer not updating when a spline moves
		[FIXED] The View Examples button in the About Curvy Window does not find the example scenes
		[FIXED] Shapes resetting themselves (and loosing all modifications done outside the Shapes menu) when Curvy reload its assemblies
		[API/NEW] Added an InterpolateAndGetTangentFast method to CurvySpline and CurvySplineSegment
		[API/CHANGE] Removed various obsolete members

5.2.2
	[FIXED] Curvy Generator: In some cases when using multiple material groups, properties of some material groups get overridden by the properties of other material groups
	[FIXED] In some complex Curvy Generators, some modules don't get updated when needed
	[FIXED] Normal computation is wrong in TRSShape
	[API/NEW] Added method SamplePointsMaterialGroup.Clone

5.2.1
	[FIXED] Build error when using Unity 2019

5.2.0
	Curvy Generator
		[NEW] Added a new module: Debug Rasterized Path
		[NEW] Input Spline Path and Input Shape Path modules: Added a "Use Global Space" option
		[CHANGE] CG module's reset now resets the listeners too
		[CHANGE] Modified the error message shown when failing to destroy a resource that is part of a prefab to make it more helpful
		[CHANGE] Debug modules now show a warning if the generator's debug mode is not active
		[CHANGE] DebugVolume now displays the path's normals too
		[OPTIM] Optimized the CG resources creation and destruction process
		[FIXED] In edit mode, automatic refresh is not respecting the set "Refresh Delay Editor" value
		[FIXED] Resetting some CG modules does not reset all their properties
		[FIXED] Newly created Templates not showing on the templates list until you close and reopen the CG window
		[FIXED] Reordering CG modules in Play mode displays an error
		[FIXED] TRS Mesh module not applying the TRS matrix as it should
		[FIXED] Operations on meshes' transforms do not update the meshes' normals and tangents correctly
		[FIXED] Shape Extrusion module: The Cross's "Shift" option doesn't work properly when its value is set to "By Orientation"
		Create Mesh CG module
			[FIXED] Exported meshes (to asset files) keep being updated even after the export operation
			[FIXED] Removing an exported mesh removes the mesh from the Curvy generator too
	Example scene 51_InfiniteTrack
		[FIXED] The generated track has discontinuities between its sections
		[FIXED] The track is material rendered weirdly
	Others
		[NEW] Added an example scene to showcase the usage of the Conform Path CG module
		[API/NEW] Added a OnGlobalCoordinatesChanged delegate to CurvySpline
	
5.1.1
	[FIXED] Projects don't build when using Unity 2017.2 or newer
	
5.1.0
	Curvy Connection
		[CHANGE] Reworked the inspector
		[CHANGE] Gizmo labels now have the same color than the icon
		[FIXED] Inspector is not displayed inside the Curvy Spline Segment inspector when deleting then re-adding a connection
		[FIXED] When having some of the control points synchronized, and some not, moving a not synchronized control point make the synchronized ones move too.
		[FIXED] When using Synchronization presets, the synchronized position/rotation is not the one of the connection's game object as it should be, but one of the synchronized control points
		[FIXED] Gizmo labels are not drawn at the right position under some circumstances
		[FIXED] Updating the synchronization options from the inspector don't apply them right away, but until one of the synchronized transforms is modified
		[FIXED] Synchronization options not working properly when moving multiple connected control points at once
		[FIXED] Gizmos are sometime drawn using the wrong color
	Curvy Generator
		[OPTIM] Various optimizations related to Shape Extrusion and Input Spline modules
		[API/FIXED] CGShape.Bounds is not filled by SplineInputModuleBase when rasterization mode is equal to GDataRequestRasterization.ModeEnum.Even
		[API/CHANGE] Made CGShape.Bounds obsolete
		[FIXED] Shape Extrusion CG module: when Optimize is true, in certain conditions the following computations give wrong results: advanced scaling, volume splitting, UV coordinates
	Controllers
		[CHANGE] "Update In" is now taken into consideration even when in Edit Mode. Note: Since in Edit Mode there no fixed updates, setting "Update In" to "Fixed Update" will be equivalent to setting it to "Late Update" while in Edit Mode
		[FIXED] Controllers not updating frequently enough while in Edit Mode.
	Others
		[FIXED] When using Unity 2018.3, opening Curvy preferences from the toolbar does not work
		[CHANGE] Made Curvy Global Manager execute before anything Curvy
		[FIXED] You can end up with multiple Curvy Global Managers when opening multiple scenes at once

5.0.0
	Import/Export splines:
		[FIXED] Deserializing JSON files with missing fields do not assign the correct default values
		[FIXED] An error is logged when closing the file selection window without selecting a file
	Curvy Preferences window:
		[FIXED] The "Reset to defaults" button int the does not reset all the preferences
		[FIXED] A warning appears when building Curvy asking to use Unity's new [SettingsProvider] attribute
	Curvy Generator:
		[NEW] Added new CG module PathRelativeTranslation. It translates a Path relatively to it's direction, instead of relatively to the world as does the TRS Path module
		[CHANGE] Volume Spots CG module: Extended the valid range of Position Offset's values to [-1,1]. Now all the volume can be covered
		[CHANGE] The "Delete Resource" button now results in an undoable action. The confirmation message was updated to reflect this change.
		[FIXED] Volume Spots CG module: crash when bounds are too small
		[FIXED] Shape Extrusion CG module: resetting the module do not reset its scaling multiplier curves
		[FIXED] TRS Path not transforming the Normals correctly
		[FIXED] When using Unity 2018.3, showing debug information of a CG module stopped it from drawing
		[FIXED] When using Unity 2018.3, error messages are logged when unloading scenes containing "Input Spline Path" or "Input Spline Shape" CG modules
		[API/NEW] Shape Extrusion CG module: Exposed the scale multiplier curves
		[API/CHANGE] Removed obsolete CGData.TimeStamp
		[API/CHANGE] CGSplineResourceLoader.Destroy and CGShapeResourceLoader.Destroy do not register undoing actions anymore. This is to be coherent with other classes implementing ICGResourceLoader
	Controllers:
		[NEW] When setting "Orientation Mode" to None, the new "Lock Rotation" option allows you to enforce the rotation to stay the same. It is active by default, to keep the same behaviour than previous versions
		[CHANGE] Spline controller: Added better error messages, especially when dealing with splines having segments of length 0
	Others:
		[OPTIM] Various minor optimizations, mostly in Curvy Generator

4.1.1
	[FIXED] Subdivision of a Bézier spline does not update the Bézier handles to keep the original spline's shape
	[FIXED] In Unity 2018.3, Curvy Generators that are an instance of a prefab don't show properly in the inspector

4.1.0
	UI Text Spline Controller
		[NEW] Added a new property, named "Static Orientation", to make the text keep it's orientation regardless of the spline it follows
		[FIXED] Inspector displays unused properties
	Curvy Generator
		[NEW] Create Mesh CG module: Exposed GameObject.tag in the "General" tab
		[NEW] Create Mesh CG module: Exposed MeshCollider.cookingOptions in the "Collider" tab
		[CHANGE] A slightly better modules reordering algorithm
		[FIXED] Saving a scene just after reordering modules does not save the modules positions
	Others:
		[OPTIM] Some splines related operations are faster thanks to C# 7.2, available when using Unity 2018.3 or above with .Net 4.x scripting runtime
		[FIXED] Incompatibility issues with Unity 2018.3 beta
		
4.0.1
	[FIXED] UI icons not showing in projects using .Net 3.5

4.0.0
	Create Mesh CG module		
		[CHANGE] Combining meshes now always produce continuous meshes
		[CHANGE] Enhanced the warnings when combining big meshes
		[OPTIM] Memory and CPU savings, especially when combining (merging) meshes and/or generating a Mesh Collider
		[FIXED] Null reference exception when combining big meshes
	Volume Spots CG module
		[NEW] Added warnings when the module is misconfigured
		[CHANGE] The Use Volume option is now hidden when irrelevant
		[CHANGE] The first available item is always added when creating groups
		[CHANGE] When using objects with no bounds (like point lights), the module will assume a depth of 1 Unity unit for those objects
		[FIXED] Various bugs and crashes happening in some situations where the module has null or empty input bounds and/or groups
		[FIXED] Invalid spots computation when using objects with no bounds (like point lights) mixed with objects with bounds		
		[FIXED] Object's bounds computation sometimes not considering the colliders
	Others:
		[FIXED] Normalize tool failed to normalize properly some Bezier splines
		[FIXED] Curvy Line Renderer not working properly when attached to an object that is not a spline
		[FIXED] Removed obsolete warnings when compiling Curvy
		[FIXED] In example scene 04_PaintSpline, the painted path is not positioned correctly
		[API/CHANGE] Made CGData.Timestamp obsolete
		[API/CHANGE] Removed the deprecated method VolumeController.ConvertObsoleteCrossPosition
		[API/FIXED] CGBounds copy constructor do not copy the source name
		
3.0.0
	Starting from this version, Curvy is using the versioning standard "Semantic Versioning 2.0.0". More about it here: https://semver.org/
	In a nutshell, and given a version number MAJOR.MINOR.PATCH:
	 -  An increase in MAJOR is done when there are non backwards-compatible changes.
	 -  An increase in MINOR is done when there are backwards-compatible changes that adds new features/API member, or modifies existing ones.
	 -  An increase in PATCH is done when there are backwards-compatible bug fixes.
	Whenever a number is increased, the numbers to its right are set to 0.

	Curvy Generator:
		[NEW] Added a  Reorder Modules" in the Curvy Generator toolbar. This will automatically sort the position of modules to make the graph easily readable
		[CHANGE] Mix Shapes and Mix Paths CG modules show warning when mixing inputs with incompatible unmixable properties
		[FIXED] Error when validating Input Spline Path or Input Spline Shape CG modules when they have a null source
		[FIXED] UI warnings in some CG modules are never cleared, even when the reason for the warning is fixed
		[FIXED] Mix Shapes CG module not working for shapes of different points counts
		[FIXED] Mix Shapes and Mix Paths CG modules giving wrong normals, directions, length, F array and bounds
		[FIXED] Create Mesh CG module can't generate a flat mesh collider
		[API/FIXED] Mix Shapes and Mix Paths CG modules produce CGShape and CGPath objects with most field set to default value
		[API/FIXED] CGShape and CGPath interpolation methods sometime returns non normalized vectors

	Others:
		[CHANGE] Removed features previously marked as deprecated
		[OPTIM] Dynamic orientation computation optimized. Up to 20% performance increase
		[FIXED] For splines long enough with control points close enough, a Spline Controller could fail to handle connections and/or send events for some control points
		[FIXED] Control Points created using pooling get potentially a different orientation from ones created without pooling
		[FIXED] Compiling Curvy shows a compilation warning
		[API/CHANGE] Corrected the typo in CurvySpline.GetSegementIndex method
		[API/CHANGE] Removed deprecated API members
		[API/CHANGE] Merged CurvySplineBase with CurvySpline
	
2.3.0
	Controllers:
		All controllers:
			[NEW] Added an OnInitialized Unity event
			[NEW] Controller's position can now be animated via Unity's Animation window, or any other animation tool
			[DEPRECATED] The "Animate" option is removed. Use Unity's Animation window or any other animation tool instead
			[DEPRECATED] "Adapt On Change" is removed, but it's behaviour is still there. Controllers will now always "adapt" (keeping their relative or absolute position, depending on the Position Mode value) when there source changes.
			[DEPRECATED] The Space parameter is no more used. The controller now works correctly without the need to set the correct Space value.
			[CHANGE] Direction is no more set from the sign of the Speed, but through a new value, Direction
			[CHANGE] When orientation's source has a value of "None", any rotation previously applied by the controller will be reverted
			[CHANGE] When orientation's source has a value of "None", Target and Ignore Direction are no more displayed in the inspector
			[CHANGE] When OrientationMode is set to None, controller now uses the object's rotation on initialization instead of keeping the last assigned rotation
			[FIXED] Wrong controller orientation when Target has a value of Backward
			[FIXED] When orientation's source has a value of "Tangent", the orientation of the spline is ignored
			[FIXED] Position is using Move Mode instead of Position Mode
			[FIXED] Wrong controller's position for 0 length splines
			[FIXED] Damping causes infinite rotations when the controller is parented to a rotated object, and has "Self" as a Space value
			[FIXED] Very high values of OffsetRadius sometimes lead to the controller stopping
			[FIXED] Offset compensation is always a frame late
			[FIXED] Offset compensation is is computed based on world speed even when move mode is set to "Relative"
			[FIXED] When OrientationMode is set to "None", the controller in some cases compensates the offset even when no offset is applied
			[API/NEW] TeleportBy and TeleportTo methods
			[API/CHANGE] Stop method now always rewinds the controller to its position it had when starting playing
			[API/CHANGE] Some properties are no more virtual
			[API/CHANGE] Moved all controllers related code inside the FluffyUnderware.Curvy.Controllers namespace
			[API/FIXED] UserAfterUpdate not being called like its documentation states
			[API/FIXED] Wrap method does not take Offset into consideration
			[API/FIXED] Apply method does not take Offset into consideration
		Spline Controller:
			[NEW] Added options in the inspector to define the controller's behaviour when reaching a Connection: continue on the current spline, use the Follow-Up, ...
			[FIXED] Adding a listener to a SplineController's event is ignored if that event had no listener when the controller is initialized
			[FIXED] Spline Controller not working properly with Catmull-Rom splines having Auto End Tangents to false
			[FIXED] Switching splines ignores Offset
			[FIXED] Switching splines ignores Damping
			[FIXED] Switch duration is ignored when On Switch event is listened to
			[FIXED] Controller has wrong position when following a moved spline while having "Self" as a Space
			[FIXED] When spline and spline controller are parented to a rotated object, rotation of the controller is wrong when having "World" as a Space
			[FIXED] When spline and spline controller are parented to a moved object, controller switching between splines has wrong position when having "World" as a Space
			[API/NEW] Added FinishCurrentSwitch and CancelCurrentSwitch methods
			[API/CHANGE] SwitchTo method now raises an error if called on a stopped controller
			[API/CHANGE] CurvySplineMoveEventArgs fields are now read only. To modify a controller movement, modify the controller properties instead of CurvySplineMoveEventArgs fields
			[API/CHANGE] CurvySplineMoveEventArgs: sender is now always a SplineController
		UI Text Spline controller:
			[FIXED] Invalid position when having "World" as a Space
		Path and Volume Controllers
			[FIXED] "Adapt On Change" option has no effect
	Curvy Splines:
		[OPTIM] Real time splines modification takes tens of percents less CPU
		[CHANGE] Transforming a spline into a shape will remove all its connections
		[CHANGE] GetTangentFast now uses slerp instead of lerp to interpolate between cached tangents
		[FIXED] Connection inspector forbids setting some Heading values when control point is connected to a closed spline's first control point
		[FIXED] Slowdowns due to some connected control points synchronizing unnecessarily their transforms every frame
		[FIXED] Events are triggered before the spline is refreshed
		[FIXED] OnAfterControlPointChanges event was sometimes send with a null Spline property
		[FIXED] OnAfterControlPointAdd and OnAfterControlPointChanges events where sometimes called with the wrong value of CurvyControlPointEventArgs.Mode
		[API/NEW] InsertBefore and InsertAfter methods's overrides that set the created control point's position
		[API/NEW] CurvySpline.GlobalCoordinatesChangedThisFrame
		[API/CHANGE] CurvySpline.DistanceToTF will now return 0 (instead of 1) when spline's length is 0
		[API/CHANGE] InsertBefore, InsertAfter and Delete methods now have an optional parameter to make them not call the Refresh method, and not trigger events
		[API/CHANGE] Start now calls Refresh, which means OnRefresh event can be send in Start method
		[API/CHANGE] Adding and deleting control points events are no more cancellable
		[API/CHANGE/FIXED] Made methods that add or removes control points from splines coherent between themselves in the following ways:
			They now all call Refresh at their end
			OnAfterControlPointChanges will always send a CurvySplineEvent
			Methods that add or remove multiple control points will now always send only one event of each relevant type
		[API/FIXED] TFToDistance and DistanceToTF behave differently when handling the TF and Distance of the last Control Point of a spline
		[API/FIXED] CurvySpline.GetTangentFast returns non normalized vectors
		[API/FIXED] DistanceToSegment returns different result that TFToSegment for the same segment
	Misc:
		[NEW] Added support for compiler symbol: CURVY_SANITY_CHECKS. It activates various sanity checks in Curvy's code. Very useful to debug your code when using the Curvy API. This symbol is not set by default. For performance reasons, please do not set this symbol when building your release binary.
		[CHANGE] Set script execution order for some script: CurvyConection < CurvySplineSegment < CurvySpline < CurvyController
		[CHANGE] CurvySplineExportWizard do not support CurvySplineGroups anymore (but still supports multiple CurvySplines)
		[CHANGE] Moved files to the Plugins folder
		[FIXED] Draw splines tool: undoing the creation of a connected Control Point gives console errors
		[FIXED] Draw Splines tool: the "Add & Smart Connect" action generates splines that have incorrect shapes
		[FIXED] CG options handled incorrectly when extruding a mesh using a Cross with non null Start PC and End CP
		[FIXED] Namespace conflict with Oculus example project
		[FIXED] ThreadPoolWorker does not use all available cores in some cases.
		
2.2.4.2
	[OPTIM] Huge optimizations related to splines modification. Real time splines modification can take up to an order of magnitude less time
	[OPTIM] Reduced multi threading computational and memory overheads
	[OPTIM] Optimized splines interpolation
	[CHANGE] Made splines initialize earlier, in Start instead of the first Update/FixedUpdate/LateUpdate
	[CHANGE] Resetting a Control Point will now keep it's connections
	[CHANGE] Connection inspector: to no allow invalid Heading settings
	[CHANGE] Curvy Spline Segment inspector: OrientationAnchor is no more visible when it's value is irrelevant
	[CHANGE] Made materials in example scenes compatible with Unity 2018 (dropping of Substance based ones)
	[CHANGE] Transformation from text to number of user provided values now uses the invariant culture instead of the culture on the user's machine. More about invariant culture here: https://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.invariantculture%28v=vs.110%29.aspx
	[FIXED] Exceptions thrown when updating splines with "Use Threading" and "Bake Orientation" in one of the control points set to true
	[FIXED] Occasional exceptions thrown when undoing some operations
	[FIXED] In Meta CG Options' inspector, setting First U from neighbors lead to thrown exceptions or invalid values
	[FIXED] Spline Controller does not work when setting its spline from code
	[FIXED] Changing spline controller's speed through the SetFromString does not work
	[FIXED] Controller's orientation can be wrong when TimeScale is set to 0 or when unpausing the game
	[FIXED] If a Control Point is used as a Follow-Up, moving it sometimes does not update the connected spline
	[FIXED] Control points that have already a next or previous Control point are allowed to have Follow-Ups
	[FIXED] A spline does not get updated when its first Control Point has a Follow-Up that moves
	[FIXED] Changing a spline's Auto End Tangents do not refresh following up spline
	[FIXED] Connection between control points at the end or start of a spline prevent those control points to become invisible when Auto End Tangents is set to false
	[FIXED] Connection persists after deletion
	[FIXED] Connections still existing even if all the connected control points are removed
	[FIXED] When changing Auto End Tangents from false to true, the Orientation Anchor value of the second control point is always overridden to true
	[FIXED] Selecting an orphan control point from a loaded scene leads to null references
	[FIXED] Spline does not get updated when one of its children Control Points is moved elsewhere via the Hierarchy window
	[FIXED] First tangent of a segment is sometimes not up to date
	[FIXED] When switching from Orientation Static or Dynamic to None, orientation for some control points is not recomputed
	[FIXED] The orientation of the last segment's cached points is sometimes not up to date
	[FIXED] A spline's Length can have a negative value when removing all control points
	[FIXED] A spline keeps a non zero value for Cache Size and Segments count even after removing all control points
	[FIXED] Sometimes, when enabling a disabled spline, the control points hierarchy is not reflecting the actual splines' control points
	[FIXED] Changing the number of points in a Spiral (in the Shape wizard) sometimes make the spiral "entangled"
	[FIXED] "Max Points Per Unit" is not correctly imported when using the splines import/export tool
	[FIXED] Resetting a CurvySpline component will set invalid value to its "Max Points Per Unit"
	[API/CHANGE] CurvySpline.Create now returns an initialized splines
	[API/CHANGE] Now all CurvySplineSegments have approximation arrays of a length of CacheSize + 1
	[API/CHANGE] Made CurvySplineBase abstract
	[API/CHANGE] CurvySplineSegment's ControlPointIndex can no longer be set. It is now handled by CurvySpline
	[API/CHANGE] Made components stripping systematic when pushing CurvySplineSegments to pools
	[API/FIXED] IsLastSegment returns true even if Control Point was not a segment
	[API/FIXED] When using Follow-Ups, GetPreviousControlPoint returns a Control Point that is not a segment even when segmentsOnly is true
	[API/FIXED] Poolable objects do not get their OnBeforePush method called before being pushed into a pool
	[API/FIXED] Setters of CurvySpline.MaxPointsPerUnit, CurvySplineGroup.Splines and BuildShapeExtrusion.CrossShiftValue did not update these values correctly
	Changes since 2.2.4.1
		[FIXED] StartCP and EndCP in InputSplinePath and InputSplineShape modules are set automatically to null.
		
2.2.3
Reminder: at each Curvy update, please delete the old Curvy folders before installing the new version.
	[NEW] Assembly definitions support
		Curvy was restructured and modified to support Unity's assembly definitions. This feature is disabled by default in this release. To enable it, search in Curvy installed folders for files with ".asmdef.disabled" extension, and rename the extension to ".asmdef".
	[NEW] CG module slots can be renamed without breaking the generator's data
	[CHANGE] Spline sampling parameters were modified:
		- The global "Max Cache PPU" is now obsolete and replaced with a per spline "Max Max Points Per Unit"
		- The "Min Distance" parameter of the Curvy Generators is now obsolete
		For more details please go here https://forum.curvyeditor.com/thread-526.html	
	[CHANGE] Fixed an example script's namespace
	[CHANGE] Added relevant dlls generated by asmdefs to link.xml
	[CHANGE] 0 is no more a valid value for the Resolution parameter in "Rasterize Path" and "Shape Extrusion" CG modules
	[CHANGE] Modified some CG module slots names to reflect the need for a rasterized path, in opposition to a non rasterized one.
	[FIXED] Some CurvyUISpline properties do not get reset properly when resetting the component
	[FIXED] Spline's level of detail is different between the editor and the build.
	[FIXED] Extruded shapes become two dimensional if small enough
	[FIXED] The "Use Cache" parameter in the spline input CG modules is ignored when linked to "Shape Extrusion" or "Rasterize Path" CG modules having "Optimize" set to true. 
	[FIXED] The rasterization resolution in the "Rasterize Path" CG module is modified if the module's Length parameter is modified.
	[FIXED] Extruded mesh jitters when modifying its path spline's length
	[FIXED] Wrong name of Rasterize Path in the CG modules list
	[FIXED, except when building against .NET Core] Curvy doesn't detect custom CG modules and Shapes from other assemblies.
	[FIXED] The Curvy generator templates list is broken in .Net 4.6
	[FIXED] In the CG graph, the suggested list of modules to connect with the current module contains modules you can't connect to
	[FIXED] Spline to Mesh tool generated spline at the wrong position
	[FIXED] Pools get duplicated if the pooled object's class is in a different assembly from the pool
	[FIXED] Multiple pools of the same component crash the game
	[FIXED] Obsolete messages from Unity 2017.3 and newer
	[FIXED] WebGL application crashes when using a spline having Use Threading set to true
	Example scenes:
		[CHANGE] Set assets serialization mode to Text
		[CHANGE] Reduced Ressource images size
		[CHANGE] Various tweaks and enhancements
		[FIXED] Example scenes do not render properly on WebGL and Mobile
		
2.2.2
	Spline to Mesh:
		[CHANGE] Renamed the "Mesh Export" tool to "Spline to Mesh" to avoid confusion.
		[CHANGE] Spline to Mesh does no more require the input spline to be on the X/Y plane
		[FIXED] Spline to Mesh does not open in recent Unity version
		[FIXED] Spline to Mesh produces wrong values in UV2.y
		
	Spline Import/Export wizard:
		[NEW] Added link to documentation in the spline Import/Export wizard
		[CHANGE] Spline Import/Export wizard modified to make it more straight forward to use
		[CHANGE] Modified the field names in the exported spline JSON format, to make it similar to the spline field names in the inspector
		[FIXED] Spline Import/Export wizard does not open in recent Unity versions
		[FIXED] "String too long" error when displaying long text in the spline Import/Export wizard TextMeshGenerator Cutting off characters

	Others:
		[Change] Replaced the usage of the obsolete UnityEditor.EditorApplication.playmodeStateChanged method with UnityEditor.EditorApplication.playModeStateChanged and UnityEditor.EditorApplication.pauseStateChanged for Unity 2017.2 and above
		[FIXED] WebGL builds fail
		[FIXED] Builds that use code stripping fail on Unity 2017.1 and older
		[FIXED] When synchronizing connected spline handles, moving a handle can invert the others
		
2.2.1
	[CHANGE] Modified the UI of the CG module "Create Mesh" to avoid confusion regarding the "Make Static" option:
		- "Make Static" is now not editable in play mode, since its Unity equivalent (GameObject.IsStatic) is an editor only property.
		- When "Make Static" is true, the other options are not editable while in play mode. This is to reflect the behaviour of the "Create Mesh" module, which is to not update the mesh while under those conditions, to avoid overriding the optimizations Unity do to static game objects' meshs.
	[FIXED] When combining multiple Volumes having different values for the "Generate UV" setting, the created mesh has invalid UVs
	[FIXED] "Mesh.normals is out of bounds" error when Generating a mesh that has Caps while using the Combine option
	[FIXED] Convex property, in CG module Create Mesh, not applied on generated mesh collider
	[FIXED] Negative SwirlTurns are ignored
	[FIXED] Orientation interpolated the wrong way (Lerping instead of SLerping)
	[FIXED] Cross's "Reverse Normal" in "Shape Extrusion" module is ignored when a "Volume Hollow" is set
	[FIXED] Crash on IOS when using code stripping on Unity 2017.2 and above.
	[Optimization] Various optimizations, the most important ones are related to "Shape Extrusion"'s normals computations and Orientation computation
	[API] Added a new GetNearestPointTF overload that also returns the nearestSegment and the nearestSegmentF
	[API] Made CrossReverseNormals, HollowInset and HollowReverseNormals properties public in BuildShapeExtrusion
	
2.2.0
	[NEW] Addressed Unity 2017.3 incompatibilities
	[NEW] Added a RendererEnabled option to the CreateMesh CG module. Useful if you generate a mesh for collider purposes only.
	[FIXED] Error when using pooling with Unity 2017.2 and above
	[FIXED] Incompatibility with UWP10 build
	[FIXED] SceneSwitcher.cs causing issues with the global namespace of Scene Switcher being occupied by the PS4's SDK
	[FIXED] Curvy crashing when compiled with the -checked compiler option
	[FIXED] TRSShape CG module not updating properly the shape's normals
	[FIXED] ReverseNormals not reversing normals in some cases
	      Note: You might have ticked "Reverse Normals" in some of your Curvy Generators, but didn't notice it because of the bug. Now that the bug is fixed, those accidental "Reverse Normals" will get activated.
	[FIXED] Split meshes not having the correct normals
	[CHANGE] Replaced website, documentation and forum URLs with the new ones.
	[Optimization] Various optimizations, the most important ones are related to mesh generation (UVs, normals and tangents computation)
	
2.1.3
	[FIXED] TimeScale affects controller movement when Animate is off
	[FIXED] Reverse spline movement going wrong under some rare conditions
	
2.1.2
	[NEW] Added CreatePathLineRenderer CG module
	[NEW] Addressed Unity 5.5 incompatibilities
	[FIXED] SplineController.AdaptOnChange failing under some conditions
	[FIXED] Selecting a spline while the Shape wizard is open immediately changes it's shape
	[FIXED] ModifierMixShapes module not generating normals
	[CHANGE] Changed 20_CGPath example to showcase CreatePathLineRenderer module
	
2.1.1
	[NEW] Added CurvySplineBase.GetApproximationPoints
	[NEW] Added Offsetting and offset speed compensation to CurvyController
	[FIXED] ImportExport toolbar button ignoring ShowGlobalToolbar option
	[FIXED] Assigning CGDataReference to VolumeController.Volume and PathController.Path fails at runtime
	[CHANGE] OrientationModeEnum and OrientationAxisEnum moved from CurvyController to FluffyUnderware.Curvy namespace
	[CHANGE] ImportExport Wizard now cuts text and logs a warning if larger then allowed by Unity's TextArea
	
2.1.0
	[NEW] More options for the Mesh Triangulation wizard
	[NEW] Improved Spline2Mesh and SplinePolyLine classes for better triangulator support
	[NEW] BuildVolumeCaps performance heavily improved
	[NEW] Added preference option to hide _CurvyGlobal_ GameObject
	[NEW] Import/Export API & wizard for JSON serialization of Splines and Control Points (Catmull-Rom & Bezier)
	[NEW] Added 22_CGClonePrefabs example scene
	[NEW] Windows Store compatibility (Universal 8.1, Universal 10)
	[FIXED] BuildVolumeMesh.KeepAspect not working properly
	[FIXED] CreateMesh.SaveToScene() not working properly
	[FIXED] NRE when using CreateMesh module's Mesh export option
	[FIXED] Spline layer always resets to default spline layer
	[FIXED] CurvySpline.TFToSegmentIndex returning wrong values
	[FIXED] SceneSwitcher helper script raise errors at some occasions
	[CHANGE] Setting CurvyController.Speed will only change movement direction if it had a value of 0 before
	[CHANGE] Dropped poly2tri in favor of LibTessDotNet for triangulation tasks
	[CHANGE] Removed all legacy components from Curvy 1.X
	[CHANGE] New Control Points now use the spline's layer
	
2.0.5
	[NEW] Added CurvyGenerator.FindModule<T>()
	[NEW] Added InputSplineShape.SetManagedShape()
	[NEW] Added 51_InfiniteTrack example scene
	[NEW] Added CurvyController.Pause()
	[NEW] Added CurvyController.Apply()
	[NEW] Added CurvyController.OnAnimationEnd event
	[NEW] Added option to select Connection GameObject to Control Point inspector
	[FIXED] UV2 calculation not working properly
	[FIXED] CurvyController.IsInitialized becoming true too early
	[FIXED] Controller Damping not working properly when moving backwards
	[FIXED] Control Point pool keeps invalid objects after scene load
	[FIXED] _CurvyGlobal_ frequently causes errors in editor when switching scenes
	[FIXED] Curve Gizmo drawing allocating memory unnecessarily
	[FIXED] SplineController allocates memory at some occasions
	[FIXED] CurvyDefaultEventHandler.UseFollowUp causing Stack Overflow/Unity crashing
	[FIXED] _CurvyGlobal_ GameObject disappearing by DontDestroyOnLoad bug introduced by Unity 5.3
	[CHANGE] UITextSplineController resets state when you disable it
	[CHANGE] CurvyGenerator.OnRefresh() now returns the first changed module in CGEventArgs.Module
	[CHANGE] Renamed CurvyControlPointEventArgs.AddMode to ModeEnum, changed content to "AddBefore","AddAfter","Delete","None"
	
2.0.4
	[FIXED] Added full Unity 5.3 compatibility
	
2.0.3
	[NEW] Added Pooling example scene
	[NEW] Added CurvyGLRenderer.Add() and CurvyGLRenderer.Delete()
	[FIXED] CG graph not refreshing properly
	[FIXED] CG module window background rendering transparent under Unity 5.2 at some occasions
	[FIXED] Precise Movement over connections causing position warps
	[FIXED] Fixed Curvy values resetting to default editor settings on upgrade
	[FIXED] Control Points not pooled when deleting spline
	[FIXED] Pushing Control Points to pool at runtime causing error
	[FIXED] Bezier orientation not updated at all occasions
	[FIXED] MetaCGOptions: Explicit U unable to influence faces on both sides of hard edges
	[FIXED] Changed UITextSplineController to use VertexHelper.Dispose() instead of VertexHelper.Clear()
	[FIXED] CurvySplineSegment.ConnectTo() fails at some occasions
	
2.0.2
	[NEW] Added range option to InputSplinePath / InputSplineShape modules
	[NEW] CG editor improvements
	[NEW] Added more Collider options to CreateMesh module
	[NEW] Added Renderer options to CreateMesh module
	[NEW] Added CurvySpline.IsPlanar(CurvyPlane) and CurvySpline.MakePlanar(CurvyPlane)
	[NEW] Added CurvyController.DampingDirection and CurvyController.DampingUp
	[FIXED] Shift ControlPoint Toolbar action fails with some Control Points
	[FIXED] IOS deployment code stripping (link.xml)
	[FIXED] Controller Inspector leaking textures
	[FIXED] Controllers refreshing when Speed==0
	[FIXED] VolumeController not using individual faces at all occasions
	[FIXED] Unity 5.2.1p1 silently introduced breaking changes in IMeshModifier
	[CHANGE] CurvyController.OrientationDamping now obsolete!
	
2.0.1
	[NEW] CG path rasterization now has a dedicated angle threshold
	[NEW] Added CurvyController.ApplyTransformPosition() and CurvyController.ApplyTransformRotation()
	[FIXED] CG not refreshing as intended in the editor
	[FIXED] CG not refreshing when changing used splines
	[FIXED] Controllers resets when changing inspector while playing
	A few minor fixes and improvements
	
2.0.0 Initial Curvy 2 release