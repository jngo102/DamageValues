using Modding;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DamageValues
{
    public class DamageValues : Mod, IGlobalSettings<Settings>
    {
        public static readonly string ImagesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // the path where the mod dll is located
        private List<Sprite> _sprites = new();
        private static int _tileSize;
        private static Settings _settings = new Settings();

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public DamageValues() : base("Damage Values") { }

        public override void Initialize()
        {
            var spriteImage = Directory.EnumerateFiles(ImagesDir).First(path => path.EndsWith("png"));
            if (spriteImage == null)
            {
                Modding.Logger.Log("Sprites for damage value text does not exists.");
                return;
            }

            _tileSize = _settings.TileSize;

            byte[] imageBytes = File.ReadAllBytes(Path.Combine(ImagesDir, spriteImage));
            var spriteTexture = new Texture2D(2, 2);
            spriteTexture.LoadImage(imageBytes);
            int k = 0;
            for (int i = spriteTexture.height - _tileSize; i >= 0; i -= _tileSize)
            {
                for (int j = 0; j < spriteTexture.width; j += _tileSize)
                {
                    var sprite = Sprite.Create(spriteTexture, new Rect(j, i, _tileSize, _tileSize), new Vector2(0.5f, 0.5f), _tileSize / 2);
                    sprite.name = k.ToString();
                    _sprites.Add(sprite);
                    k++;
                }
            }

            On.HealthManager.TakeDamage += OnTakeDamage;
        }

        private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);

            int damage = (int)(hitInstance.DamageDealt * hitInstance.Multiplier);
            var damageValue = new GameObject("Damage Value");
            int numDigits = 0;
            do
            {
                int i = damage % 10;
                var digit = new GameObject("Digit " + i);
                digit.transform.SetParent(damageValue.transform, false);
                digit.transform.position += Vector3.left * numDigits;
                var sr = digit.AddComponent<SpriteRenderer>();
                sr.sprite = _sprites[i];
                sr.material = new Material(Shader.Find("Sprites/Default"));

                damage /= 10;
                numDigits++;
            }
            while (damage > 0);

            damageValue.AddComponent<DamageValue>();
            damageValue.transform.SetPosition2D(self.transform.position + Vector3.right * (numDigits - 1) / 2 + Vector3.up * 2);
        }

        public void OnLoadGlobal(Settings s) => _settings = s;

        public Settings OnSaveGlobal() => _settings;
    }
}
