/*

AvatarDresser - a simple script to apply an item of clothing to your avatar.

Copyright (c) 2022 SophieBlue

*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

[ExecuteInEditMode]
public class AvatarDresser {
    private Transform _armature;
    private VRCExpressionParameters _avatarParameters;

    private VRCAvatarDescriptor _avatar;
    private GameObject _article;

    // bones we've already visited
    private HashSet<string> _visitedBones = new HashSet<string>();

    // armature bones
    private Dictionary<string, Transform> targetBones = new Dictionary<string, Transform>();

    public AvatarDresser() {
        Undo.undoRedoPerformed += AssetDatabase.SaveAssets;
    }

    public void setAvatar(VRCAvatarDescriptor avatar) {
        _avatar = avatar;
    }

    public void setArticle(GameObject article) {
        _article = article;
    }

    //
    // sourceBone = the bone in question
    // sourceBones = array of all the article's bones
    // mesh = the mesh
    //
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
        if (targetBones.TryGetValue(name, out targetBone)) {

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
            Debug.Log("Bone " + name + " parenting to " + targetBone.gameObject.name);

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
            // that's totally okay
            Debug.Log("Bone " + name + " not found in armature - handle manually");
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
            targetBones.Add(bone.gameObject.name, bone);
        });


        // Get the article's meshes
        SkinnedMeshRenderer[] articleMeshRenderers =
            _article.GetComponentsInChildren<SkinnedMeshRenderer>();

        // find each of the skinned mesh renderers in this article
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

            // assign the part's bones
            part.bones = sourceBones;

            Undo.SetTransformParent(part.gameObject.transform,
                _avatar.gameObject.transform, part.name);
        }

        Undo.DestroyObjectImmediate(_article);
        Undo.CollapseUndoOperations(undoGroupIndex);
    }
}
