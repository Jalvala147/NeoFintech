document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll("[data-nf-toggle]").forEach(function (button) {
    button.addEventListener("click", function () {
      var targetId = button.getAttribute("data-nf-toggle");
      var panel = document.getElementById(targetId);
      if (!panel) {
        return;
      }
      panel.classList.toggle("is-open");
      var expanded = panel.classList.contains("is-open");
      button.setAttribute("aria-expanded", expanded ? "true" : "false");
    });
  });
});
