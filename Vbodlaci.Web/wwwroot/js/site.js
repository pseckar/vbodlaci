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

  const defaultsScript = document.getElementById("courseDefaultsJson");
  if (defaultsScript) {
    const defaults = JSON.parse(defaultsScript.textContent || "[]");
    const byKey = new Map(defaults.map((item) => [`${item.type}:${item.field}`, item.text || ""]));
    const typeSelect = document.querySelector("[data-course-type-select]");
    const fields = {
      ShortDescription: document.getElementById("Input_ShortDescription"),
      FullDescription: document.getElementById("Input_FullDescription"),
      WhatToExpect: document.getElementById("Input_WhatToExpect")
    };

    const getDefault = (type, field) => byKey.get(`${type}:${field}`) || "This is placeholder for default text";
    let previousType = typeSelect ? typeSelect.value : "Breathwork";

    if (typeSelect) {
      typeSelect.addEventListener("change", () => {
        const nextType = typeSelect.value;
        Object.entries(fields).forEach(([field, input]) => {
          if (!input) {
            return;
          }

          const previousDefault = getDefault(previousType, field);
          if (input.value.trim() === previousDefault.trim()) {
            input.value = getDefault(nextType, field);
          }
        });

        previousType = nextType;
      });
    }

    const modalElement = document.getElementById("defaultTextModal");
    const modalType = document.getElementById("defaultTextType");
    const modalField = document.getElementById("defaultTextField");
    const modalText = document.getElementById("defaultTextValue");
    const modalTitle = document.getElementById("defaultTextModalLabel");
    const modal = modalElement && window.bootstrap ? new bootstrap.Modal(modalElement) : null;

    document.querySelectorAll(".default-text-trigger").forEach((button) => {
      button.addEventListener("click", () => {
        const selectedType = typeSelect ? typeSelect.value : "Breathwork";
        const selectedField = button.getAttribute("data-default-field") || "ShortDescription";

        if (modalType) {
          modalType.value = selectedType;
        }
        if (modalField) {
          modalField.value = selectedField;
        }
        if (modalText) {
          modalText.value = getDefault(selectedType, selectedField);
        }
        if (modalTitle) {
          modalTitle.textContent = `Upravit výchozí text: ${selectedType}`;
        }

        if (modal) {
          modal.show();
        }
      });
    });
  }

  document.querySelectorAll("[data-copy]").forEach((button) => {
    button.addEventListener("click", async () => {
      const target = document.querySelector(button.getAttribute("data-copy"));
      if (!target) {
        return;
      }

      const text = ("value" in target && target.value ? target.value : target.textContent || "").trim();
      try {
        await navigator.clipboard.writeText(text);
        const originalLabel = button.textContent;
        button.textContent = "Zkopírováno ✓";
        button.disabled = true;
        setTimeout(() => {
          button.textContent = originalLabel;
          button.disabled = false;
        }, 1500);
      } catch {
        window.prompt("Zkopíruj text ručně:", text);
      }
    });
  });

  const imagePreviewInput = document.querySelector("[data-image-preview-input]");
  const imagePreview = document.querySelector("[data-image-preview]");
  if (imagePreviewInput && imagePreview) {
    imagePreviewInput.addEventListener("change", () => {
      const file = imagePreviewInput.files && imagePreviewInput.files[0];
      if (file && file.type.startsWith("image/")) {
        imagePreview.src = URL.createObjectURL(file);
      }
    });
  }

  const bindFlashOverlay = (overlay) => {
    const closeFlash = () => overlay.remove();
    overlay.addEventListener("click", (event) => {
      if (event.target === overlay) {
        closeFlash();
      }
    });

    const closeButton = overlay.querySelector("[data-flash-close]");
    if (closeButton) {
      closeButton.addEventListener("click", closeFlash);
      closeButton.focus();
    }

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        closeFlash();
      }
    });
  };

  const initialFlashOverlay = document.querySelector("[data-flash-overlay]");
  if (initialFlashOverlay) {
    bindFlashOverlay(initialFlashOverlay);
  }

  // public forms submit in place: the outcome dialog is lifted from the
  // response so the page neither reloads nor scrolls; on validation errors
  // the returned form (same id) replaces the current one
  const bindAjaxFlashForm = (form) => {
    form.addEventListener("submit", async (event) => {
      event.preventDefault();

      if (window.jQuery && jQuery.fn.validate && !jQuery(form).valid()) {
        return;
      }

      const submitButton = form.querySelector('button[type="submit"]');
      if (submitButton) {
        submitButton.disabled = true;
      }

      try {
        const response = await fetch(form.action, {
          method: "POST",
          body: new FormData(form),
          headers: { "X-Requested-With": "fetch" }
        });
        const html = await response.text();
        const parsed = new DOMParser().parseFromString(html, "text/html");
        const overlay = parsed.querySelector("[data-flash-overlay]");
        const replacement = form.id ? parsed.getElementById(form.id) : null;

        if (!overlay && replacement) {
          replacement.setAttribute("data-ajax-flash", "");
          form.replaceWith(replacement);
          if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
            jQuery.validator.unobtrusive.parse(replacement);
          }
          bindAjaxFlashForm(replacement);
          return;
        }

        if (overlay) {
          if (response.ok) {
            form.reset();
          }
          document.body.appendChild(overlay);
          bindFlashOverlay(overlay);
          return;
        }

        window.location.href = response.url || window.location.href;
      } catch {
        form.submit();
      } finally {
        if (submitButton) {
          submitButton.disabled = false;
        }
      }
    });
  };

  document.querySelectorAll("form[data-ajax-flash]").forEach(bindAjaxFlashForm);

  const revealElements = Array.from(document.querySelectorAll(".reveal"));
  if (revealElements.length > 0) {
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (reducedMotion || !("IntersectionObserver" in window)) {
      revealElements.forEach((element) => element.classList.add("in"));
    } else {
      const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("in");
            observer.unobserve(entry.target);
          }
        });
      }, { threshold: 0.12 });

      revealElements.forEach((element) => observer.observe(element));
    }
  }
})();
