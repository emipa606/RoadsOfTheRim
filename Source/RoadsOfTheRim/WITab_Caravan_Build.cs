﻿using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace RoadsOfTheRim
{
    public class WITab_Caravan_Build : WITab
    {
        private float scrollViewHeight;

        private Vector2 scrollPosition;

        private List<Pawn> Pawns => SelCaravan.PawnsListForReading;
        public WITab_Caravan_Build()
        {
            labelKey = "RoadsOfTheRim_WITab_Caravan_Build";
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            var rect2 = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
            var num = 0f;
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            DoColumnHeaders(ref num);
            DoRows(ref num, rect2, rect);
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = num + 30f;
            }
            Widgets.EndScrollView();
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();
            size = GetRawSize();
        }

        private void DoColumnHeaders(ref float curY)
        {
            Text.Anchor = TextAnchor.UpperCenter;
            GUI.color = Widgets.SeparatorLabelColor;
            Widgets.Label(new Rect(135f, 3f, 100f, 100f), "Work"); // TO TRANSLATE
            Widgets.Label(new Rect(255f, 3f, 100f, 100f), "Skill"); // TO TRANSLATE
            Widgets.Label(new Rect(375f, 3f, 100f, 100f), "Best road"); // TO TRANSLATE
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private void DoRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
        {
            List<Pawn> pawns = Pawns;
            var flag = false;
            for (var i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!pawn.IsColonist)
                {
                    continue;
                }
                if (!flag)
                {
                    Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanColonists".Translate());
                    flag = true;
                }
                DoRow(ref curY, scrollViewRect, scrollOutRect, pawn);
            }
            var flag2 = false;
            for (var j = 0; j < pawns.Count; j++)
            {
                Pawn pawn2 = pawns[j];
                if (pawn2.IsColonist)
                {
                    continue;
                }
                if (!flag2)
                {
                    Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanPrisonersAndAnimals".Translate());
                    flag2 = true;
                }
                DoRow(ref curY, scrollViewRect, scrollOutRect, pawn2);
            }
        }

        private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
        {
            var num = scrollPosition.y - 50f;
            var num2 = scrollPosition.y + scrollOutRect.height;
            if (curY > num && curY < num2)
            {
                DoRow(new Rect(0f, curY, viewRect.width, 50f), p);
            }
            curY += 50f;
        }


        private void DoRow(Rect rect, Pawn p)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = rect.AtZero();
            CaravanThingsTabUtility.DoAbandonButton(rect2, p, SelCaravan);
            rect2.width -= 24f;
            Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, p);
            rect2.width -= 24f;
            if (Mouse.IsOver(rect2))
            {
                Widgets.DrawHighlight(rect2);
            }
            var rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
            Widgets.ThingIcon(rect3, p, 1f);
            var bgRect = new Rect(rect3.xMax + 4f, 16f, 100f, 18f);
            GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, false, false);
            var num = bgRect.xMax;
            for (var i = 0; i < 3; i++)
            {
                var rect5 = new Rect(num, 0f, 100f, 50f);
                if (Mouse.IsOver(rect5))
                {
                    Widgets.DrawHighlight(rect5);
                }
                Text.Anchor = TextAnchor.MiddleCenter;
                string s;
                switch (i)
                {
                    case 0:
                        s = PawnBuildingUtility.ShowConstructionValue(p);
                        break;
                    case 1:
                        s = PawnBuildingUtility.ShowSkill(p);
                        break;
                    case 2:
                        s = PawnBuildingUtility.ShowBestRoad(p);
                        break;
                    default:
                        s = "-";
                        break;
                }
                Widgets.Label(rect5, s);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                TooltipHandler.TipRegion(rect5, s);
                num += 125f;
            }

            if (p.Downed)
            {
                GUI.color = new Color(1f, 0f, 0f, 0.5f);
                Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
                GUI.color = Color.white;
            }
            GUI.EndGroup();
        }


        private Vector2 GetRawSize()
        {
            var num = 500f;
            Vector2 result;
            result.x = 127f + num + 16f;
            result.y = Mathf.Min(550f, PaneTopY - 30f);
            return result;
        }
    }
}
