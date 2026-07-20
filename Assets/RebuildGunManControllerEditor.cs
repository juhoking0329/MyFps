#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

public class RebuildGunManControllerEditor
{
    public static void Execute()
    {
        string controllerPath = "Assets/MyFps/Enemy/Robot/GunMan.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogError("Could not find GunMan.controller");
            return;
        }

        // 1. Parameters Cleanup
        var pList = controller.parameters.ToList();
        var isFiringParam = pList.FirstOrDefault(p => p.name == "IsFiring");
        if (isFiringParam != null)
        {
            controller.RemoveParameter(isFiringParam);
        }

        // 2. Base Layer
        AnimatorStateMachine baseSm = controller.layers[0].stateMachine;
        
        // Clear all AnyState transitions (except maybe we will re-add Death)
        baseSm.anyStateTransitions = new AnimatorStateTransition[0];

        AnimatorState idle = GetState(baseSm, "Idle");
        AnimatorState walk = GetState(baseSm, "Walk");
        AnimatorState attack = GetState(baseSm, "Attack");
        AnimatorState death = GetState(baseSm, "Death");
        AnimatorState patrol = GetState(baseSm, "Walk With Rifle");
        if (patrol == null) patrol = GetState(baseSm, "Patrol");

        if (idle == null || walk == null || attack == null || death == null || patrol == null)
        {
            Debug.LogError("Some states are missing in Base Layer!");
            return;
        }

        // Clear all state transitions
        idle.transitions = new AnimatorStateTransition[0];
        walk.transitions = new AnimatorStateTransition[0];
        attack.transitions = new AnimatorStateTransition[0];
        death.transitions = new AnimatorStateTransition[0];
        patrol.transitions = new AnimatorStateTransition[0];

        // Entry -> Walk With Rifle (Patrol)
        baseSm.defaultState = patrol;

        // Base Layer Transitions
        AddTransition(patrol, idle, "EnemyState", AnimatorConditionMode.Equals, 0);
        AddTransition(patrol, walk, "EnemyState", AnimatorConditionMode.Equals, 1);
        
        AddTransition(walk, patrol, "EnemyState", AnimatorConditionMode.Equals, 4);
        AddTransition(walk, attack, "EnemyState", AnimatorConditionMode.Equals, 2);

        AddTransition(attack, walk, "EnemyState", AnimatorConditionMode.Equals, 1);
        AddTransition(attack, patrol, "EnemyState", AnimatorConditionMode.Equals, 4);

        AddTransition(idle, patrol, "EnemyState", AnimatorConditionMode.Equals, 4);
        AddTransition(idle, walk, "EnemyState", AnimatorConditionMode.Equals, 1);

        // Any State -> Death
        var tDeath = baseSm.AddAnyStateTransition(death);
        tDeath.AddCondition(AnimatorConditionMode.Equals, 3, "EnemyState");
        tDeath.hasFixedDuration = true;
        tDeath.duration = 0.25f;

        // 3. UpperBody Layer
        AnimatorControllerLayer[] layers = controller.layers;
        int upperIndex = -1;
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].name == "UpperBody") upperIndex = i;
        }

        if (upperIndex != -1)
        {
            AnimatorStateMachine upperSm = layers[upperIndex].stateMachine;

            // Clear all AnyState transitions
            upperSm.anyStateTransitions = new AnimatorStateTransition[0];

            AnimatorState empty = GetState(upperSm, "Empty");
            AnimatorState aiming = GetState(upperSm, "Aiming");
            if (aiming == null) aiming = GetState(upperSm, "Rifle Aiming Idle");
            AnimatorState firing = GetState(upperSm, "Firing");
            if (firing == null) firing = GetState(upperSm, "Firing Rifle");

            // Clear all state transitions
            empty.transitions = new AnimatorStateTransition[0];
            aiming.transitions = new AnimatorStateTransition[0];
            firing.transitions = new AnimatorStateTransition[0];

            upperSm.defaultState = empty;

            // Empty -> Aiming (1 or 2)
            AddTransition(empty, aiming, "EnemyState", AnimatorConditionMode.Equals, 1);
            AddTransition(empty, aiming, "EnemyState", AnimatorConditionMode.Equals, 2);

            // Aiming -> Empty (4 or 0)
            AddTransition(aiming, empty, "EnemyState", AnimatorConditionMode.Equals, 4);
            AddTransition(aiming, empty, "EnemyState", AnimatorConditionMode.Equals, 0);

            // Aiming -> Firing (Trigger)
            var tToFire = aiming.AddTransition(firing);
            tToFire.AddCondition(AnimatorConditionMode.If, 0, "FireTrigger");
            tToFire.hasExitTime = false;
            tToFire.hasFixedDuration = true;
            tToFire.duration = 0.1f;

            // Firing -> Aiming (Exit Time)
            var tToAim = firing.AddTransition(aiming);
            tToAim.hasExitTime = true;
            tToAim.exitTime = 0.8f;
            tToAim.hasFixedDuration = true;
            tToAim.duration = 0.25f;
            
            // Reassign layer to save
            controller.layers = layers;
            AssetDatabase.SaveAssets();
            Debug.Log("GunMan Controller successfully rebuilt according to Claude's spec!");
        }
        else
        {
            Debug.LogError("UpperBody layer not found.");
        }

        FileUtil.DeleteFileOrDirectory("Assets/RebuildGunManControllerEditor.cs");
    }

    private static AnimatorState GetState(AnimatorStateMachine sm, string name)
    {
        foreach (var child in sm.states)
        {
            if (child.state.name.Contains(name)) return child.state;
        }
        return null;
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, string param, AnimatorConditionMode mode, float val)
    {
        var t = from.AddTransition(to);
        t.AddCondition(mode, val, param);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = 0.25f;
    }
}
#endif
