using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace FromGoldenCombs.Items
{
    class LangstrothPartCore : Item
    {

        /// <summary>Called by the inventory system when you hover over an item stack. This is the item stack name that is getting displayed.</summary>
        /// <param name="itemStack"></param>
        /// <returns>
        ///   Computed string showing the materials the item is made from.
        /// </returns>
        public override string GetHeldItemName(ItemStack itemStack)
        {
            
            if (itemStack.Collectible.Variant["accent"] != null)
            {
                return this.VariantStrict["primary"].ToString().UcFirst() + "-" + this.Variant["accent"].ToString().UcFirst() + " " + base.GetHeldItemName(itemStack).ToString();
            }
            else
            {
                return this.VariantStrict["primary"].ToString().UcFirst() + " " + base.GetHeldItemName(itemStack).ToString();
            }

        }
    }
}
