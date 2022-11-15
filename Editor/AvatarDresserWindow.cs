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

    private GameObject _article;
    private VRCAvatarDescriptor _avatar;

    // Our class with the real logic
    private AvatarDresser avatarDresser = new AvatarDresser();

    [MenuItem ("Window/Avatar Dresser")]
    public static void ShowWindow() {
        // Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(AvatarDresserWindow));
    }

    // Gives a description regarding what the plugin does
    private void Header() {
        GUIStyle styleTitle = new GUIStyle(GUI.skin.label);
        styleTitle.fontSize = 16;
        styleTitle.margin = new RectOffset(20, 20, 20, 20);
        EditorGUILayout.LabelField("Avatar Dresser", styleTitle);
        EditorGUILayout.Space();

        GUIStyle styleVersion = new GUIStyle(GUI.skin.label);
        EditorGUILayout.LabelField(version, styleVersion);
        EditorGUILayout.Space();
    }

    // Sets up all the baseline information
    private void MainOptions() {
        // The Avatar
        _avatar = EditorGUILayout.ObjectField(
            "Avatar", _avatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;

        // The article of clothing
        _article = EditorGUILayout.ObjectField(
            "Article", _article, typeof(GameObject), true) as GameObject;
    }

    // Apply all the options back to the main AvatarDresser class
    private void ApplyOptions() {
        avatarDresser.setAvatar(_avatar);
        avatarDresser.setArticle(_article);
    }

    void OnGUI() {
        Header();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        MainOptions();
        ApplyOptions();

        if (GUILayout.Button("Apply Changes")) {
            avatarDresser.Apply();
        }

        EditorGUILayout.EndScrollView();
    }
}
