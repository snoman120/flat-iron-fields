using System.Collections.Generic;
using FlatIronFields.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FlatIronFields.Input
{
    public enum ActionId
    {
        Interact,
        CycleCropNext,
        CycleCropPrev,
        CycleAttachment,
        NextDay,
        OpenSeeds,
        OpenSell,
        OpenLand,
        OpenTractor,
        OpenSkills,
        OpenQuests,
        OpenUpgrades,
        ShowHelp,
        Tool1,
        Tool2,
        Tool3,
        Tool4,
        Tool5,
        Tool6
    }

    public sealed class InputRouter : MonoBehaviour
    {
        private readonly Dictionary<ActionId, bool> pressed = new Dictionary<ActionId, bool>();

        public InputState Current { get; private set; } = new InputState();

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) { return; }

            float horizontal = Axis(keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
                                    keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
            float vertical = Axis(keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
                                  keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);

            Current = new InputState
            {
                Horizontal = horizontal,
                Vertical = vertical,
                Interact = keyboard.spaceKey.wasPressedThisFrame
            };

            pressed.Clear();
            pressed[ActionId.Interact] = keyboard.spaceKey.wasPressedThisFrame;
            pressed[ActionId.CycleCropNext] = keyboard.eKey.wasPressedThisFrame;
            pressed[ActionId.CycleCropPrev] = keyboard.qKey.wasPressedThisFrame;
            pressed[ActionId.CycleAttachment] = keyboard.tKey.wasPressedThisFrame;
            pressed[ActionId.NextDay] = keyboard.nKey.wasPressedThisFrame;
            pressed[ActionId.OpenSeeds] = keyboard.bKey.wasPressedThisFrame;
            pressed[ActionId.OpenSell] = keyboard.uKey.wasPressedThisFrame;
            pressed[ActionId.OpenLand] = keyboard.mKey.wasPressedThisFrame;
            pressed[ActionId.OpenTractor] = keyboard.vKey.wasPressedThisFrame;
            pressed[ActionId.OpenSkills] = keyboard.kKey.wasPressedThisFrame;
            pressed[ActionId.OpenQuests] = keyboard.jKey.wasPressedThisFrame;
            pressed[ActionId.OpenUpgrades] = keyboard.rKey.wasPressedThisFrame;
            pressed[ActionId.ShowHelp] = keyboard.hKey.wasPressedThisFrame;
            pressed[ActionId.Tool1] = keyboard.digit1Key.wasPressedThisFrame;
            pressed[ActionId.Tool2] = keyboard.digit2Key.wasPressedThisFrame;
            pressed[ActionId.Tool3] = keyboard.digit3Key.wasPressedThisFrame;
            pressed[ActionId.Tool4] = keyboard.digit4Key.wasPressedThisFrame;
            pressed[ActionId.Tool5] = keyboard.digit5Key.wasPressedThisFrame;
            pressed[ActionId.Tool6] = keyboard.digit6Key.wasPressedThisFrame;
        }

        public bool WasPressed(ActionId action)
        {
            bool value;
            return pressed.TryGetValue(action, out value) && value;
        }

        private static float Axis(bool positive, bool negative)
        {
            if (positive && negative) { return 0f; }
            if (positive) { return 1f; }
            if (negative) { return -1f; }
            return 0f;
        }
    }
}
