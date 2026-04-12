(() => {
  const button = document.getElementById("backToTop");
  if (!button) {
    return;
  }

  const onScroll = () => {
    if (window.scrollY > 320) {
      button.classList.add("visible");
    } else {
      button.classList.remove("visible");
    }
  };

  button.addEventListener("click", () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  });

  window.addEventListener("scroll", onScroll);
  onScroll();
})();
