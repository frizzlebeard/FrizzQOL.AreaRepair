using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AreaRepair
{
    [HarmonyPatch(typeof(Player), "Repair", typeof(ItemDrop.ItemData), typeof(Piece))]
    internal static class RepairPatch
    {
        private const float CellSize = 16f;

        private static readonly RepairIndex<Piece> Index = new RepairIndex<Piece>(CellSize);
        private static readonly List<Piece> Nearby = new List<Piece>();
        private static readonly FieldInfo GhostLayerField = AccessTools.Field(typeof(Piece), "s_ghostLayer");

        private static bool _busy;
        private static bool _watchHover;
        private static bool _hoverAccepted;
        private static int _ghostLayer = int.MinValue;

        // Player.Repair is void and returns early when the hover repair is rejected.
        // The hovered WearNTear.Repair result is the only accept signal.
        internal static void NoteHoverRepair(bool accepted)
        {
            if (_watchHover)
            {
                _hoverAccepted = accepted;
            }
        }

        internal static void Track(Piece piece)
        {
            if (piece == null)
            {
                return;
            }

            Vector3 pos = piece.transform.position;
            Index.Add(piece, pos.x, pos.y, pos.z);
        }

        internal static void Untrack(Piece piece)
        {
            Index.Remove(piece);
        }

        private static void Prefix()
        {
            if (_busy)
            {
                return;
            }

            _watchHover = true;
            _hoverAccepted = false;
        }

        private static void Postfix(Player __instance)
        {
            if (_busy)
            {
                return;
            }

            _watchHover = false;
            if (!RepairRules.ShouldAreaRepair(_hoverAccepted) || __instance == null)
            {
                return;
            }

            Piece target = __instance.GetHoveringPiece();
            if (target == null)
            {
                return;
            }

            float radius = Plugin.CurrentRadius();
            if (radius <= 0f)
            {
                return;
            }

            try
            {
                _busy = true;
                RepairNearby(target, radius);
            }
            finally
            {
                _busy = false;
            }
        }

        private static void Finalizer()
        {
            _watchHover = false;
        }

        private static void RepairNearby(Piece target, float radius)
        {
            Vector3 origin = target.transform.position;
            Index.Collect(origin.x, origin.y, origin.z, radius, Nearby);

            int repaired = 0;
            try
            {
                for (int i = 0; i < Nearby.Count; i++)
                {
                    Piece piece = Nearby[i];
                    if (piece == null || piece == target || IsGhost(piece))
                    {
                        continue;
                    }

                    Vector3 pos = piece.transform.position;
                    float distance = Vector3.Distance(origin, pos);
                    bool wardAllows = PrivateArea.CheckAccess(pos, 0f, false, false);
                    if (!RepairRules.ShouldRepairNearby(distance, radius, wardAllows))
                    {
                        continue;
                    }

                    WearNTear wear = piece.GetComponent<WearNTear>();
                    if (wear == null)
                    {
                        continue;
                    }

                    if (wear.Repair())
                    {
                        repaired++;
                    }
                }
            }
            finally
            {
                Nearby.Clear();
            }

            if (repaired > 0)
            {
                Plugin.LogInfo("Repaired " + repaired + " extra pieces.");
            }
        }

        private static bool IsGhost(Piece piece)
        {
            if (_ghostLayer == int.MinValue)
            {
                object value = GhostLayerField != null ? GhostLayerField.GetValue(null) : null;
                _ghostLayer = value is int layer ? layer : -1;
            }

            return _ghostLayer >= 0 && piece.gameObject.layer == _ghostLayer;
        }
    }

    [HarmonyPatch(typeof(WearNTear), "Repair")]
    internal static class WearRepairResultPatch
    {
        private static void Postfix(bool __result)
        {
            RepairPatch.NoteHoverRepair(__result);
        }
    }

    [HarmonyPatch(typeof(Piece), "Awake")]
    internal static class PieceTrackPatch
    {
        private static void Postfix(Piece __instance)
        {
            RepairPatch.Track(__instance);
        }
    }

    [HarmonyPatch(typeof(Piece), "OnDestroy")]
    internal static class PieceUntrackPatch
    {
        private static void Postfix(Piece __instance)
        {
            RepairPatch.Untrack(__instance);
        }
    }
}
