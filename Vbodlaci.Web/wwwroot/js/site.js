(() => {
  const button = document.getElementById("backToTop");
  if (button) {
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
  }

  const filterRoot = document.querySelector("[data-course-filter-root]");
  if (filterRoot) {
    const cards = Array.from(document.querySelectorAll(".course-card[data-course-type]"));
    if (cards.length > 0) {
      const pills = Array.from(filterRoot.querySelectorAll("[data-course-filter]"));
      const emptyState = document.querySelector("[data-course-filter-empty]");
      let selected = (filterRoot.getAttribute("data-selected-type") || "all").toLowerCase();
      if (selected !== "breathwork" && selected !== "kone") {
        selected = "all";
      }

      const applyFilter = () => {
        let visibleCount = 0;

        cards.forEach((card) => {
          const cardType = (card.getAttribute("data-course-type") || "").toLowerCase();
          const visible = selected === "all" || cardType === selected;
          card.hidden = !visible;
          if (visible) {
            visibleCount += 1;
          }
        });

        pills.forEach((pill) => {
          const value = (pill.getAttribute("data-course-filter") || "all").toLowerCase();
          pill.classList.toggle("active", value === selected);
          pill.setAttribute("aria-pressed", value === selected ? "true" : "false");
        });

        if (emptyState) {
          emptyState.hidden = visibleCount > 0;
        }
      };

      pills.forEach((pill) => {
        pill.addEventListener("click", () => {
          const value = (pill.getAttribute("data-course-filter") || "all").toLowerCase();
          selected = value;
          applyFilter();
        });
      });

      applyFilter();
    }
  }

  const dirtyForms = Array.from(document.querySelectorAll("form[data-dirty-form]"));
  dirtyForms.forEach((form) => {
    form.dataset.dirty = "false";
    form.dataset.submitting = "false";

    const markDirty = () => {
      form.dataset.dirty = "true";
    };

    form.addEventListener("input", markDirty);
    form.addEventListener("change", markDirty);
    form.addEventListener("submit", () => {
      form.dataset.submitting = "true";
    });
  });

  const dirtyLinks = Array.from(document.querySelectorAll("[data-confirm-dirty]"));
  dirtyLinks.forEach((link) => {
    link.addEventListener("click", (event) => {
      const formId = link.getAttribute("data-dirty-form-id");
      if (!formId) {
        return;
      }

      const form = document.getElementById(formId);
      if (!form) {
        return;
      }

      if (form.dataset.submitting === "true") {
        return;
      }

      if (form.dataset.dirty !== "true") {
        return;
      }

      const message = link.getAttribute("data-confirm-message") || "Máš neuložené změny. Opravdu chceš pokračovat?";
      if (!window.confirm(message)) {
        event.preventDefault();
      }
    });
  });
})();
