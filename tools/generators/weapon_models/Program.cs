using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WeaponPageCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var depsFolder = "../../deps/gta-v-data-dumps/";
            var weaponsJson = depsFolder + "weapons.json";

            var weaponsModelsFile = "../../../articles/weapons/models.md";
            var imagePath = "~/altv-docs-assets/altv-docs-gta/images/weapon/models/";

            var skipDlcTag = "g9ec"; //Generation 9 Enhanced Content

            /*
             * Read JSON files from gta-v-data-dumps by DurtyFree
             */


            if (!File.Exists(weaponsJson))
            {
                Console.WriteLine("Couldn't find " + weaponsJson + ". Are you running the project with 'dotnet run .\\generators-weapon_models.csproj'?");
                Environment.Exit(2);
            }

            using var readerWeapons = new StreamReader(weaponsJson);
            var jsonWeapons = readerWeapons.ReadToEnd();
            var weapons = JsonConvert.DeserializeObject<List<Weapon>>(jsonWeapons);

            var sortedWeaponsByName = weapons.OrderBy(x => x.Name).ToList().Where(x => !x.DlcName.Contains(skipDlcTag));
            var sortedWeaponsByNameSkipped = weapons.OrderBy(x => x.Name).ToList().Where(x => x.DlcName.Contains(skipDlcTag));

            var weaponCategories = new List<string>();
            foreach (var weapon in weapons)
            {
                if (weapon.Category == null || weapon.Category.Length == 0)
                    continue;
                
                if(weapon.TranslatedLabel == null || weapon.TranslatedLabel.English == null || weapon.TranslatedLabel.English == "Invalid")
                    continue;

                if(weapon.IsVehicleWeapon)
                    continue;

                var category = weapon.Category.Replace("GROUP_", "");

                if(!weaponCategories.Contains(category))
                {
                    weaponCategories.Add(category);
                }
            }

            weaponCategories = weaponCategories.OrderBy(x => x).ToList();

            var filteredWeapons = new List<Weapon>();

            /*
             * Generate Images
             */
            var gallery = File.CreateText(weaponsModelsFile);

            gallery.WriteLine("<!--- THIS IS AN AUTOGENERATED FILE. DO NOT EDIT THIS FILE DIRECTLY. -->");
            gallery.WriteLine("<!--- This page gets generated with tools/deps/generators/weapon_models -->");
            gallery.WriteLine("# Weapon Models");

            gallery.WriteLine("> [!TIP]");
            gallery.WriteLine("> Informations provided by <a href='https://forge.plebmasters.de/peds'>Pleb Masters: Forge</a>");

            foreach (var weaponCategory in weaponCategories)
            {
                gallery.WriteLine("### " + weaponCategory);
                gallery.WriteLine();
                gallery.WriteLine("<div class=\"grid-container\">");

                var weaponsByClass = sortedWeaponsByName.Where(x => x.Category == weaponCategory.Insert(0, "GROUP_"));
                foreach (Weapon weapon in weaponsByClass)
                {
                    if(weapon.TranslatedLabel.English == null || weapon.TranslatedLabel.English == "Invalid")
                        continue;

                    if(weapon.IsVehicleWeapon)
                        continue;

                    filteredWeapons.Add(weapon);

                    gallery.WriteLine("  <div class=\"grid-item\">");

                    var weaponPath = imagePath + weapon.Name.ToLower();

                    gallery.WriteLine($"    <div class=\"grid-item-img\">");
                    gallery.WriteLine($"      <img src=\"{weaponPath}.png\" alt=\"Missing image &quot;{weapon.Name}.png&quot;\" title=\"{weapon.Name}\" loading=\"lazy\" />");
                    gallery.WriteLine($"    </div>");

                    if (weapon.DlcName.ToLower() == "titleupdate")
                    {
                        gallery.WriteLine("    <b>Name:</b> " + weapon.Name + "<br/>");
                        gallery.WriteLine("    <b>Hash:</b> " + weapon.Hash.ToString("X").Insert(0, "0x") + "<br/>");
                        gallery.WriteLine("    <b>Display Name:</b> " + weapon.TranslatedLabel.English );
                    }
                    else
                    {
                        gallery.WriteLine("    <b>Name:</b> " + weapon.Name + "<br/>");
                        gallery.WriteLine("    <b>Hash:</b> " + weapon.Hash.ToString("X").Insert(0, "0x") + "<br/>");
                        gallery.WriteLine("    <b>Display Name:</b> " + weapon.TranslatedLabel.English + "<br/>");
                        gallery.WriteLine("    <b>DLC:</b> " + weapon.DlcName.ToLower());
                    }

                    gallery.WriteLine("  </div>");
                }

                gallery.WriteLine("</div>");
                gallery.WriteLine();
            }

            filteredWeapons = filteredWeapons.OrderBy(x => x.Name).ToList();

            /*
             * Generate Snippets
             */
            gallery.WriteLine();
            gallery.WriteLine("## Snippets");
            gallery.WriteLine();

            gallery.WriteLine("# [JavaScript](#tab/tab1-0)");
            gallery.WriteLine("```js");
            gallery.WriteLine("export class WeaponModel {");

            foreach (var weapon in filteredWeapons)
            {

                gallery.WriteLine($"  static {weapon.Name.ToLower()} = {weapon.Hash.ToString("X").Insert(0, "0x")};");
            }

            gallery.WriteLine("}");
            gallery.WriteLine("```");
            gallery.WriteLine("# [TypeScript](#tab/tab1-1)");
            gallery.WriteLine("```ts");
            gallery.WriteLine("export enum WeaponModel {");

            foreach (var weapon in filteredWeapons)
            {
                gallery.WriteLine($"  {weapon.Name.ToLower()} = {weapon.Hash.ToString("X").Insert(0, "0x")},");
            }

            gallery.WriteLine("}");
            gallery.WriteLine("```");
            gallery.WriteLine("***");

            gallery.WriteLine();

            gallery.WriteLine("**Created with [GTA V Data Dumps from DurtyFree](https://github.com/DurtyFree/gta-v-data-dumps)**");

            gallery.Close();
            Console.WriteLine($"{weaponsModelsFile} created for {filteredWeapons.Count} weapons.");

            Console.WriteLine("This tool is using data files from https://github.com/DurtyFree/gta-v-data-dumps");
        }
    }
}
