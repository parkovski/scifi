using UnityEngine;
using System;

namespace SciFi {
    [Serializable]
    public enum Team {
        None,
        One,
        Two,
    }

    public static class TeamColor {
        public static readonly Color blueTeamColor = new Color(0.5f, 0.5f, 1f, 1f);
        public static readonly Color blueTeamColorDark = new Color(0f, 0f, .6f, 1f);
        public static readonly Color redTeamColor = new Color(1f, .4f, .4f, 1f);
        public static readonly Color redTeamColorDark = new Color(.6f, 0f, 0f, 1f);
        public static readonly Color greenTeamColor = new Color(.1f, .6f, .1f, 1f);
        public static readonly Color greenTeamColorDark = new Color(0, .4f, 0f, 1f);
        public static readonly Color yellowTeamColor = new Color(1f, 1f, .4f, 1f);
        public static readonly Color yellowTeamColorDark = new Color(0.6f, 0.6f, 0f, 1f);

        public static Color FromIndex(int index, bool dark = false) {
            switch (index) {
            case 0:
                return dark ? blueTeamColorDark : blueTeamColor;
            case 1:
                return dark ? redTeamColorDark : redTeamColor;
            case 2:
                return dark ? greenTeamColorDark : greenTeamColor;
            case 3:
                return dark ? yellowTeamColorDark : yellowTeamColor;
            default:
                return Color.clear;
            }
        }
    }
}