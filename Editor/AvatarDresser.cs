/*

AvatarDresser - a simple script to apply an item of clothing to your avatar.

Copyright (c) 2022 SophieBlue

*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace SophieBlue.AvatarDresser {

    [ExecuteInEditMode]
    public class AvatarDresser {
        public const string AssetFolderParent = "SophieBlue";
        public const string AnimationFolder = "_generatedAnimations";

        private Transform _armature;
        private VRCExpressionParameters _avatarParameters;

        // data from the user
        private VRCAvatarDescriptor _avatar;
        private GameObject _article;
        private bool _createAnimations;
        private VRCExpressionsMenu _menu;

        // bones we've already visited
        private HashSet<string> _visitedBones = new HashSet<string>();

        // armature bones
        private Dictionary<string, Transform> targetBones = new Dictionary<string, Transform>();

        public AvatarDresser() {
            Undo.undoRedoPerformed += AssetDatabase.SaveAssets;
        }

        // setters
        public void setAvatar(VRCAvatarDescriptor avatar) {
            _avatar = avatar;
        }

        public void setArticle(GameObject article) {
            _article = article;
        }

        public void setCreateAnimations(bool status) {
            _createAnimations = status;
        }

        public void setMenu(VRCExpressionsMenu menu) {
            _menu = menu;
        }

        // find the FX animation controller
        private AnimatorController getFXController() {
            VRCAvatarDescriptor.CustomAnimLayer fx = Array.Find(_avatar.baseAnimationLayers,
                l => l.type == VRCAvatarDescriptor.AnimLayerType.FX);
            return fx.animatorController as AnimatorController;
        }

        /// <summary>
        /// Ensures the specified asset folder exists under our parent,
        /// creating the path if need be
        /// </summary>
        private void ensureAssetFolder(string name) {
            if (! AssetDatabase.IsValidFolder($"Assets/{AssetFolderParent}")) {
                AssetDatabase.CreateFolder("Assets", AssetFolderParent);
            }
            if (! AssetDatabase.IsValidFolder($"Assets/{AssetFolderParent}/{name}")) {
                AssetDatabase.CreateFolder($"Assets/{AssetFolderParent}", name);
            }
        }

        /// <summary>
        /// Create toggle animations for this clothing part
        /// </summary>
        /// <param name="mesh">The SkinnedMeshRender which is the clothing item</param>
        ///
        /// the process:
        ///  creates enable and disable animations
        ///  creates an animation layer in the avatar's FX animator
        ///  creates a menu parameter in the avatar's parameters
        ///  creates a toggle in the provided menu
        private void createToggleAnimations(SkinnedMeshRenderer mesh) {
            AnimatorController fxController = getFXController();
            if (! fxController) {
                Debug.LogWarning("Avatar has no FX controller - not adding toggle animations.");
                return;
            }
            string name = mesh.gameObject.name;
            string parameterName = $"Outfit/{name}";

            ensureAssetFolder(AnimationFolder);

            // create the animation clips
            AnimationClip toggleAnimOn = createToggleAnimation(mesh, true);
            AnimationClip toggleAnimOff = createToggleAnimation(mesh, false);

            addItemToggleLayer(fxController, parameterName, toggleAnimOn, toggleAnimOff);

            // create the menu parameter and toggle
            addBoolParameter(parameterName);
            addMenuToggle(name, parameterName);

            AssetDatabase.SaveAssets();
        }

        ///
        /// <summary>
        /// Creates a toggle animation for the article
        /// </summary>
        /// <param name="mesh">the SkinnedMeshRenderer which is the clothing item</param>
        /// <param name="status">true to create enable animation, false for disable</status>
        ///
        private AnimationClip createToggleAnimation(SkinnedMeshRenderer mesh, bool status) {
            string name = mesh.gameObject.name;
            string statusString = status ? "enable" : "disable";
            string filename = $"Assets/{AssetFolderParent}/{AnimationFolder}/{name}-{statusString}.anim";

            // get or create this clip
            AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(filename);
            if (! animationClip) {
                animationClip = new AnimationClip();
                AssetDatabase.CreateAsset(animationClip, filename);
            }

            // enable or disable in the first keyframe as appropriate
            Keyframe key = new Keyframe(0.0f, status ? 1.0f : 0.0f);
            AnimationCurve animationCurve = new AnimationCurve(key);
            animationClip.SetCurve(name, typeof(GameObject), "m_IsActive", animationCurve);

            return animationClip;
        }


        /// <summary>
        /// Adds an item toggle layer to the given AnimationController
        /// </summary>
        /// <param name="fxController">The FX AnimationController</param>
        /// <param name="name">The name of the new layer</param>
        /// <param name="enableClip">The animation clip to enable the item</param>
        /// <param name="disableClip">The animation clip to disable the item</param>
        private void addItemToggleLayer(
            AnimatorController fxController,
            string name,
            AnimationClip enableClip,
            AnimationClip disableClip) {

            // create the parameter if it doesn't exist
            if (Array.Find(fxController.parameters, (parameter) => {
                    return parameter.name == name;
                }) == null) {

                fxController.AddParameter(name, AnimatorControllerParameterType.Bool);
            }

            Undo.RegisterCompleteObjectUndo(fxController, "Animator Controller");

            // create animator layer
            AnimatorControllerLayer layer = new AnimatorControllerLayer {
                defaultWeight = 1.0f,
                name = name,
                stateMachine = new AnimatorStateMachine()
            };

            // our states, one for each of on and off
            AnimatorStateMachine stateMachine = layer.stateMachine;
            AnimatorState stateOff = stateMachine.AddState("Disabled");
            stateOff.motion = disableClip;
            AnimatorState stateOn = stateMachine.AddState("Enabled");
            stateOn.motion = enableClip;

            // conditions and transition from off to on
            AnimatorCondition enableCondition = new AnimatorCondition {
                mode = AnimatorConditionMode.If,
                parameter = name
            };

            AnimatorCondition[] enableConditions = { enableCondition };
            AnimatorStateTransition enableTransition = new AnimatorStateTransition {
                conditions = enableConditions,
                destinationState = stateOn,
                duration = 0.0f,
                hasExitTime = true,
                exitTime = 0.1f
            };
            stateOff.AddTransition(enableTransition);

            // conditions and transition from on to off
            AnimatorCondition disableCondition = new AnimatorCondition {
                mode = AnimatorConditionMode.IfNot,
                parameter = name
            };
            AnimatorCondition[] disableConditions = { disableCondition };
            AnimatorStateTransition disableTransition = new AnimatorStateTransition {
                conditions = disableConditions,
                destinationState = stateOff,
                duration = 0.0f,
                hasExitTime = true,
                exitTime = 0.1f
            };
            stateOn.AddTransition(disableTransition);

            // save all the things
            string animatorControllerPath = AssetDatabase.GetAssetPath(fxController);
            AssetDatabase.AddObjectToAsset(stateMachine, animatorControllerPath);
            AssetDatabase.AddObjectToAsset(stateOn, animatorControllerPath);
            AssetDatabase.AddObjectToAsset(stateOff, animatorControllerPath);
            AssetDatabase.AddObjectToAsset(enableTransition, animatorControllerPath);
            AssetDatabase.AddObjectToAsset(disableTransition, animatorControllerPath);

            fxController.AddLayer(layer);
        }

        /// <summary>
        /// Add a boolean parameter to the Avatar's parameters
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="defaultValue">default value</param>
        /// <param name="saved">whether this parameter is saved</param>
        private void addBoolParameter(string name, float defaultValue = 0.0f, bool saved = true) {

            VRCExpressionParameters parameters = _avatar.expressionParameters;

            // return if it's already there
            if (parameters.FindParameter(name) != null) {
                return;
            }
            Undo.RegisterCompleteObjectUndo(parameters, "Target Avatar Parameters");

            // copy the list
            int count = parameters.parameters.Length;
            VRCExpressionParameters.Parameter[] newParameters = new VRCExpressionParameters.Parameter[count
     + 1];
            for (int i = 0; i < count; i++) {
                newParameters[i] = parameters.GetParameter(i);
            }

            // make the new parameter
            VRCExpressionParameters.Parameter p = new VRCExpressionParameters.Parameter {
                name = name,
                valueType = VRCExpressionParameters.ValueType.Bool,
                defaultValue = defaultValue,
                saved = saved
            };
            newParameters[count] = p;

            // set the list in the avatar
            _avatar.expressionParameters.parameters = newParameters;
        }

        /// <summary>
        /// Adds a toggle to the menu
        /// </summary>
        /// <param name="name">menu item name</param>
        /// <param name="parameterName">parameter name</param>
        private void addMenuToggle(string name, string parameterName) {
            // make sure it's not already here
            foreach (VRCExpressionsMenu.Control control in _menu.controls) {
                if (control.name == name) {
                    return;
                }
            }

            Undo.RegisterCompleteObjectUndo(_menu, "Target Menu");

            VRCExpressionsMenu.Control toggle = new VRCExpressionsMenu.Control {
                name = name,
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter {
                    name = parameterName
                }
            };
            _menu.controls.Add(toggle);
        }

        /// <summary>
        /// Recurse down the add-on's armature and re-parent it to the avatar's armature
        /// </summary>
        /// <param name="sourceBone">The bone we're currently working on</param>
        /// <param name="sourceBones">Array of all the article's bones</param>
        /// <param name="mesh">the SkinnedMeshRenderer which is the new item</param>
        Transform[] recurseBones(
                    Transform sourceBone,
                    Transform[] sourceBones,
                    ref SkinnedMeshRenderer mesh) {

            string name = sourceBone.gameObject.name;

            // if we've seen this one again, skip it
            if (_visitedBones.Contains(name)) {
                return sourceBones;
            }
            _visitedBones.Add(name);

            // found it?  Great!  We'll reparent this, then recurse
            Transform targetBone;

            //if (targetBones.TryGetValue(name, out targetBone)) {
            if (BoneMapper.TryFindBone(name, targetBones.Keys(), out targetBone)) {

                // is it the root bone?  Let's properly set this
                if (mesh.rootBone == sourceBone) {
                    // set new bone in the mesh
                    mesh.rootBone = targetBone;
                }

                // recurse down into each child bone
                foreach (Transform child in sourceBone) {

                    // Get direct descendants only
                    if (child.parent == sourceBone) {

                        // find this child in the bones list, so we can re-parent it
                        int sc = Array.FindIndex(sourceBones, x => x == child);
                        if (sc >= 0 && sc < sourceBones.Length) {
                            // recurse into this bone
                            sourceBones = recurseBones(child, sourceBones, ref mesh);
                            if (sourceBones[sc].parent == sourceBone) {
                                // parent this child to our target bone
                                Undo.SetTransformParent(child, targetBone, name);
                            }
                        }
                    }
                }

                // reassign the bone
                int s = Array.FindIndex(sourceBones, x => x == sourceBone);
                sourceBones[s] = targetBone;
            }

            // okay... probably it's reasonably parented then
            else if (targetBones.TryGetValue(sourceBone.parent.gameObject.name, out targetBone)) {
                //Debug.Log("Bone " + name + " parenting to " + targetBone.gameObject.name);

                // set the bone's parent to the *armature* bone
                Undo.SetTransformParent(sourceBone, targetBone, sourceBone.gameObject.name);

                // recurse down into each child bone
                foreach (Transform child in sourceBone) {

                    // recurse into direct descendents only
                    if (child.parent == sourceBone) {
                        sourceBones = recurseBones(child, sourceBones, ref mesh);
                    }
                }

                // reassign the bone
                int s = Array.FindIndex(sourceBones, x => x == sourceBone);
                sourceBones[s] = sourceBone;
            }

            // bones we don't have in the armature we'd have to just bail out on and
            // tell the user they'll need to do something manual
            else {
                // TODO: see if the bone has already been moved under the armature,
                // that's totally okay, otherwise we may need to alert about these:
                //Debug.Log("Bone " + name + " not found in armature - you may have to handle it manually");
            }

            return sourceBones;
        }


        [ContextMenu("Apply Clothing")]
        public void Apply() {
            if (_avatar == null) {
                Debug.LogError("You must assign a target avatar descriptor!");
                return;
            }
            if (_article == null) {
                Debug.LogError("You must choose a clothing prefab!");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Avatar Dresser");
            int undoGroupIndex = Undo.GetCurrentGroup();

            // Create a duplicate article that isn't a prefab and delete the
            // prefab, so it can be returned on undo
            string articleName = _article.gameObject.name;
            GameObject article = UnityEngine.Object.Instantiate(_article);
            Undo.RegisterCreatedObjectUndo(article, "Article");
            Undo.DestroyObjectImmediate(_article);
            _article = article;

            // Get the armature
            _armature = _avatar.transform.Find("Armature");
            _avatarParameters = _avatar.expressionParameters;

            // find all bones in the armature, make them a dictionary for convenience
            List<Transform> targetBoneList = new List<Transform>(_armature.GetComponentsInChildren<Transform>());
            targetBones.Clear();
            targetBoneList.ForEach(delegate(Transform bone) {
                if (!targetBones.ContainsKey(bone.gameObject.name)) {
                    targetBones.Add(bone.gameObject.name, bone);
                }
            });


            // Get the article's meshes
            SkinnedMeshRenderer[] articleMeshRenderers =
                _article.GetComponentsInChildren<SkinnedMeshRenderer>();

            // We're dealing with a prefab that contains armature and skinned mesh renderers,
            // so we'll find each of the meshes and apply them to the avatar
            for (int a = 0; a < articleMeshRenderers.Length; a++) {
                SkinnedMeshRenderer part = articleMeshRenderers[a];
                Undo.RecordObject(part.gameObject, "Article");

                //Debug.Log("Working on mesh " + part.gameObject.name);

                Transform[] sourceBones = part.bones;

                // recurse through the bones in this mesh
                _visitedBones.Clear();
                for (int s = 0; s < sourceBones.Length; s++) {
                    sourceBones = recurseBones(sourceBones[s], sourceBones, ref part);
                }

                // set the mesh's root bone, if not set
                if (part.rootBone == null) {
                    targetBoneList.ForEach(delegate(Transform bone) {
                        if (bone.parent == _armature && part.rootBone == null) {
                            part.rootBone = bone;
                        }
                    });
                }

                // assign this part's updated bones
                part.bones = sourceBones;

                Undo.SetTransformParent(part.gameObject.transform, _avatar.gameObject.transform, part.name);

                // optionally create animations to toggle this part on or off
                if (_createAnimations) {
                    createToggleAnimations(part);
                }
            }

            Undo.DestroyObjectImmediate(_article);
            Undo.CollapseUndoOperations(undoGroupIndex);
        }
    }
}
