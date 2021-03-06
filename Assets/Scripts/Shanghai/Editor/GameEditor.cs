﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Game))]
public class GameEditor : UnityEditor.Editor
{
    Game game;
    void OnEnable()
    {
        game = (Game)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("ShuffleOneStep"))
        {
            if (game.shufflingList.Count == 0)
            {
                Debug.Log("洗好了");
                return;
            }

            game.ShuffleOneStep();
            SceneView.RepaintAll();
        }
    }
}