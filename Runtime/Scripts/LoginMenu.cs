using BRIJ.Code.Models;
using TMPro;
using UnityEngine;

namespace BRIJ
{
    public class LoginMenu : MonoBehaviour
    {
        public BrijMain brij;
        public TMP_InputField usernameField;
        public TMP_InputField passwordField;
        public TMP_Text infoText;

        public void OnLoginClick()
        {
            if (usernameField.text.Length == 0)
            {
                infoText.text = "Username/Email cannot be empty!";
                return;
            }
            
            if (passwordField.text.Length == 0)
            {
                infoText.text = "Username/Email cannot be empty!";
                return;
            }
            
            StartCoroutine(BRIJ.Api.TryLogin(usernameField.text, passwordField.text, brij.gameToken, OnSuccessfulLogin, OnLoginFailed));
            passwordField.text = string.Empty;
        }

        public void OnSignUpClick()
        {
            Api.OpenRegisterPage();
        }

        private void OnSuccessfulLogin(GameSessionModel session)
        {
            brij.Login(session);
            gameObject.SetActive(false);
        }

        private void OnLoginFailed(string text)
        {
            infoText.text = text;
        }
    }
}