/*

AvatarDresser - a simple script to apply an item of clothing to your avatar.

Copyright (c) 2022 SophieBlue

*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public class AvatarDresserWindow : EditorWindow
{
    private String version = "v0.1.0";
    private Vector2 scroll;

    // data from the user
    private GameObject _article;
    private VRCAvatarDescriptor _avatar;
    private bool _createAnimations;
    private VRCExpressionsMenu _menu;

    // Our class with the real logic
    private AvatarDresser avatarDresser = new AvatarDresser();


    [MenuItem ("Window/SophieBlue/Avatar Dresser")]
    public static void ShowWindow() {
        // Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(AvatarDresserWindow));
    }

    private void Header() {
        GUIStyle styleTitle = new GUIStyle(GUI.skin.label);
        styleTitle.fontSize = 16;
        styleTitle.margin = new RectOffset(20, 20, 20, 20);
        EditorGUILayout.LabelField("Sophie's Avatar Dresser", styleTitle);
        EditorGUILayout.Space();

        GUIStyle styleVersion = new GUIStyle(GUI.skin.label);
        EditorGUILayout.LabelField(version, styleVersion);
        EditorGUILayout.Space();
    }

    private void MainOptions() {
        // The Avatar
        _avatar = EditorGUILayout.ObjectField(
            "Avatar", _avatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;

        // The article of clothing
        _article = EditorGUILayout.ObjectField(
            "Clothing prefab", _article, typeof(GameObject), true) as GameObject;

        // Toggle for creating animations
        _createAnimations = EditorGUILayout.Toggle("Create Animations", _createAnimations);

        if (_createAnimations) {
            // use the top level menu by default
            if (_avatar && ! _menu) {
                _menu = _avatar.expressionsMenu;
            }
            _menu = EditorGUILayout.ObjectField(
                "Menu", _menu, typeof(VRCExpressionsMenu), true) as VRCExpressionsMenu;
        }
    }

    private void ApplyOptions() {
        avatarDresser.setAvatar(_avatar);
        avatarDresser.setArticle(_article);
        avatarDresser.setCreateAnimations(_createAnimations);
        avatarDresser.setMenu(_menu);
    }

    void OnGUI() {
        Header();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        MainOptions();
        ApplyOptions();

        if (GUILayout.Button("Get Dressed!")) {
            avatarDresser.Apply();
        }

        EditorGUILayout.EndScrollView();
    }
}
