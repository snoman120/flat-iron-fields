using FlatIronFields.Core;
using UnityEngine;

namespace FlatIronFields.UI
{
    public sealed class HudController : MonoBehaviour
    {
        public UIManager Manager;
        public GameState State;

        private void LateUpdate()
        {
            if (Manager == null || State == null) { return; }
            Manager.RenderHud(State);
        }
    }
}
