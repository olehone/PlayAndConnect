const form = document.querySelector("form"),
  usernameField = form.querySelector(".username-field"),
  usernameInput = usernameField.querySelector(".username"),
  passField = form.querySelector(".create-password"),
  passInput = passField.querySelector(".password");

function checkUsername() {
  if (usernameInput.value.trim() === "") {
    usernameField.classList.add("invalid");
  } else {
    usernameField.classList.remove("invalid");
  }
}


const eyeIcons = document.querySelectorAll(".show-hide");

eyeIcons.forEach((eyeIcon) => {
  eyeIcon.addEventListener("click", () => {
    const pInput = eyeIcon.parentElement.querySelector("input");
    if (pInput.type === "password") {
      eyeIcon.classList.replace("bx-hide", "bx-show");
      return (pInput.type = "text");
    }
    eyeIcon.classList.replace("bx-show", "bx-hide");
    pInput.type = "password";
  });
});

function checkPassword() {
  const passPattern =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
  if (!passInput.value.match(passPattern)) {
    passField.classList.add("invalid");
  } else {
    passField.classList.remove("invalid");
  }
}

// Form Submit
form.addEventListener("submit", (e) => {
  e.preventDefault();
  checkUsername();
  checkPassword();

  usernameInput.addEventListener("keyup", checkUsername);
  passInput.addEventListener("keyup", checkPassword);

  if (
    !usernameField.classList.contains("invalid") &&
    !passField.classList.contains("invalid")
  ) {
    location.href = form.getAttribute("action");
  }
});