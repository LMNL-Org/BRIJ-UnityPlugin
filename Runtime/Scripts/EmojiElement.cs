using BRIJ.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BRIJ
{
    public class EmojiElement : MonoBehaviour
    {
        public Image image;
        public TMP_Text countText;
        public BrijMain brij;

        private string _name;
        private PostMenu _menu;
        private PostModel _post;
        private PostReactionModel _reaction = null;

        public string Name { get => _name; set => _name = value; }
        public PostModel Post { get => _post; set => _post = value; }
        public PostReactionModel Reaction { get => _reaction; set => _reaction = value; }
        public PostMenu Menu { get => _menu; set => _menu = value; }

        public void OnClick()
        {
            brij.OnEmojiClick(this);
        }
    }
}