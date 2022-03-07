using Paradise.Core.Types;
using System;
using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class LoadoutView {
		public LoadoutView() {
			this.Type = AvatarType.LutzRavinoff;
			this.SkinColor = string.Empty;
		}

		public LoadoutView(int loadoutId, int backpack, int boots, int cmid, int face, int functionalItem1, int functionalItem2, int functionalItem3, int gloves, int head, int lowerBody, int meleeWeapon, int quickItem1, int quickItem2, int quickItem3, AvatarType type, int upperBody, int weapon1, int weapon1Mod1, int weapon1Mod2, int weapon1Mod3, int weapon2, int weapon2Mod1, int weapon2Mod2, int weapon2Mod3, int weapon3, int weapon3Mod1, int weapon3Mod2, int weapon3Mod3, int webbing, string skinColor) {
			this.Backpack = backpack;
			this.Boots = boots;
			this.Cmid = cmid;
			this.Face = face;
			this.FunctionalItem1 = functionalItem1;
			this.FunctionalItem2 = functionalItem2;
			this.FunctionalItem3 = functionalItem3;
			this.Gloves = gloves;
			this.Head = head;
			this.LoadoutId = loadoutId;
			this.LowerBody = lowerBody;
			this.MeleeWeapon = meleeWeapon;
			this.QuickItem1 = quickItem1;
			this.QuickItem2 = quickItem2;
			this.QuickItem3 = quickItem3;
			this.Type = type;
			this.UpperBody = upperBody;
			this.Weapon1 = weapon1;
			this.Weapon1Mod1 = weapon1Mod1;
			this.Weapon1Mod2 = weapon1Mod2;
			this.Weapon1Mod3 = weapon1Mod3;
			this.Weapon2 = weapon2;
			this.Weapon2Mod1 = weapon2Mod1;
			this.Weapon2Mod2 = weapon2Mod2;
			this.Weapon2Mod3 = weapon2Mod3;
			this.Weapon3 = weapon3;
			this.Weapon3Mod1 = weapon3Mod1;
			this.Weapon3Mod2 = weapon3Mod2;
			this.Weapon3Mod3 = weapon3Mod3;
			this.Webbing = webbing;
			this.SkinColor = skinColor;
		}

		public int LoadoutId { get; set; }

		public int Backpack { get; set; }

		public int Boots { get; set; }

		public int Cmid { get; set; }

		public int Face { get; set; }

		public int FunctionalItem1 { get; set; }

		public int FunctionalItem2 { get; set; }

		public int FunctionalItem3 { get; set; }

		public int Gloves { get; set; }

		public int Head { get; set; }

		public int LowerBody { get; set; }

		public int MeleeWeapon { get; set; }

		public int QuickItem1 { get; set; }

		public int QuickItem2 { get; set; }

		public int QuickItem3 { get; set; }

		public AvatarType Type { get; set; }

		public int UpperBody { get; set; }

		public int Weapon1 { get; set; }

		public int Weapon1Mod1 { get; set; }

		public int Weapon1Mod2 { get; set; }

		public int Weapon1Mod3 { get; set; }

		public int Weapon2 { get; set; }

		public int Weapon2Mod1 { get; set; }

		public int Weapon2Mod2 { get; set; }

		public int Weapon2Mod3 { get; set; }

		public int Weapon3 { get; set; }

		public int Weapon3Mod1 { get; set; }

		public int Weapon3Mod2 { get; set; }

		public int Weapon3Mod3 { get; set; }

		public int Webbing { get; set; }

		public string SkinColor { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[LoadoutView: [Backpack: ");
			stringBuilder.Append(this.Backpack);
			stringBuilder.Append("][Boots: ");
			stringBuilder.Append(this.Boots);
			stringBuilder.Append("][Cmid: ");
			stringBuilder.Append(this.Cmid);
			stringBuilder.Append("][Face: ");
			stringBuilder.Append(this.Face);
			stringBuilder.Append("][FunctionalItem1: ");
			stringBuilder.Append(this.FunctionalItem1);
			stringBuilder.Append("][FunctionalItem2: ");
			stringBuilder.Append(this.FunctionalItem2);
			stringBuilder.Append("][FunctionalItem3: ");
			stringBuilder.Append(this.FunctionalItem3);
			stringBuilder.Append("][Gloves: ");
			stringBuilder.Append(this.Gloves);
			stringBuilder.Append("][Head: ");
			stringBuilder.Append(this.Head);
			stringBuilder.Append("][LoadoutId: ");
			stringBuilder.Append(this.LoadoutId);
			stringBuilder.Append("][LowerBody: ");
			stringBuilder.Append(this.LowerBody);
			stringBuilder.Append("][MeleeWeapon: ");
			stringBuilder.Append(this.MeleeWeapon);
			stringBuilder.Append("][QuickItem1: ");
			stringBuilder.Append(this.QuickItem1);
			stringBuilder.Append("][QuickItem2: ");
			stringBuilder.Append(this.QuickItem2);
			stringBuilder.Append("][QuickItem3: ");
			stringBuilder.Append(this.QuickItem3);
			stringBuilder.Append("][Type: ");
			stringBuilder.Append(this.Type);
			stringBuilder.Append("][UpperBody: ");
			stringBuilder.Append(this.UpperBody);
			stringBuilder.Append("][Weapon1: ");
			stringBuilder.Append(this.Weapon1);
			stringBuilder.Append("][Weapon1Mod1: ");
			stringBuilder.Append(this.Weapon1Mod1);
			stringBuilder.Append("][Weapon1Mod2: ");
			stringBuilder.Append(this.Weapon1Mod2);
			stringBuilder.Append("][Weapon1Mod3: ");
			stringBuilder.Append(this.Weapon1Mod3);
			stringBuilder.Append("][Weapon2: ");
			stringBuilder.Append(this.Weapon2);
			stringBuilder.Append("][Weapon2Mod1: ");
			stringBuilder.Append(this.Weapon2Mod1);
			stringBuilder.Append("][Weapon2Mod2: ");
			stringBuilder.Append(this.Weapon2Mod2);
			stringBuilder.Append("][Weapon2Mod3: ");
			stringBuilder.Append(this.Weapon2Mod3);
			stringBuilder.Append("][Weapon3: ");
			stringBuilder.Append(this.Weapon3);
			stringBuilder.Append("][Weapon3Mod1: ");
			stringBuilder.Append(this.Weapon3Mod1);
			stringBuilder.Append("][Weapon3Mod2: ");
			stringBuilder.Append(this.Weapon3Mod2);
			stringBuilder.Append("][Weapon3Mod3: ");
			stringBuilder.Append(this.Weapon3Mod3);
			stringBuilder.Append("][Webbing: ");
			stringBuilder.Append(this.Webbing);
			stringBuilder.Append("][SkinColor: ");
			stringBuilder.Append(this.SkinColor);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
