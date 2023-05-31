This page is a demo that shows everything you can do inside pages and blog posts.

We've included everything you need to create engaging posts about your work, and show off your case studies in a beautiful way.

**Obviously,** we’ve styled up *all the basic* text formatting options [available in markdown](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet).

You can create lists:

* Simple bulleted lists
* Like this one
* Are cool

And:

1. Numbered lists
2. Like this other one
3. Are great too

You can also add blockquotes, which are shown at a larger width to help break up the layout and draw attention to key parts of your content:

> “Simple can be harder than complex: You have to work hard to get your thinking clean to make it simple. But it’s worth it in the end because once you get there, you can move mountains.”

The theme also supports markdown tables: To apply styling to it, be sure to apply a class by adding {: .table .table-responsive } below the table. More options for table styling can be found in the [Argon Design System Pro documentation here](https://demos.creative-tim.com/argon-design-system-pro/).

| Item                 | Author        | Supports tables? | Price |
|----------------------|---------------|------------------|-------|
| Duet Jekyll Theme    | Jekyll Themes | Yes              | $49   |
| Index Jekyll Theme   | Jekyll Themes | Yes              | $49   |
| Journal Jekyll Theme | Jekyll Themes | Yes              | $49   |
{: .table .table-responsive}

And footnotes[^1], which link to explanations[^2] at the bottom of the page[^3].

[^1]: Beautiful modern, minimal theme design.
[^2]: Powerful features to show off your work.
[^3]: Maintained and supported by the theme developer.

You can throw in some horizontal rules too:

---

### Image galleries

You can use the slider from the Argon Design System by adding HTML to your posts like below:

*For help with this functionality, [see the Argon Design System Pro documentation here](https://demos.creative-tim.com/argon-design-system-pro/).*

   <section style="position:relative">
      <div class="blogGlide fullWidth gliderMargin">
        <div class="glide__track" data-glide-el="track">
          <ul class="glide__slides">
            <li class="glide__slide">
              <img src="../assets/img/theme/sofia-kuniakina.jpg">
            </li>
            <li class="glide__slide">
              <img src="../assets/img/theme/sacha-styles.jpg">
            </li>
            <li class="glide__slide">
              <img src="../assets/img/theme/victor-garcia.jpg">
            </li>
            <li class="glide__slide">
              <img src="../assets/img/theme/doyoun-seo.jpg">
            </li>
            <li class="glide__slide">
              <img src="../assets/img/theme/ayo-ogunseinde.jpg">
            </li>
          </ul>
        </div>
        <div class="glide__arrows d-flex justify-content-center mt-4 position-static" data-glide-el="controls">
          <button class="glide__arrow text-default position-static" data-glide-dir="<"><i class="ni ni-bold-left"></i></button>
          <button class="glide__arrow text-default position-static" data-glide-dir=">"><i class="ni ni-bold-right"></i></button>
        </div>
      </div>
    </section>

Just copy the HTML markup below and swap the images:

```html
<div class="blogGlide fullWidth">
    <div class="glide__track" data-glide-el="track">
        <ul class="glide__slides">
        <li class="glide__slide">
            <img src="sofia-kuniakina.jpg">
        </li>
        <li class="glide__slide">
            <img src="victor-garcia.jpg">
        </li>
        </ul>
    </div>
    <div class="glide__arrows d-flex justify-content-center mt-2" data-glide-el="controls">
          <button class="glide__arrow text-default position-static" data-glide-dir="<"><i class="ni ni-bold-left"></i></button>
          <button class="glide__arrow text-default position-static" data-glide-dir=">"><i class="ni ni-bold-right"></i></button>
    </div>
</div>
```

*See what we did there? Code and syntax highlighting is built-in too!*

Change the number inside the 'columns' setting to create different types of gallery for all kinds of purposes. You can even click on each image to seamlessly enlarge it on the page.

---

### What about videos?

Videos are an awesome way to show off your work in a more engaging and personal way, and we’ve made sure they work great on our themes. Just paste an embed code from YouTube or Vimeo, and the theme makes sure it displays perfectly:

<iframe src="https://player.vimeo.com/video/88357807?color=6c6e95&title=0&byline=0" width="640" height="360" frameborder="0" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>

---

### What about a contact form?

We've made a contact form that you can use with [Formspree](https://formspree.io/create/jekyllthemes) to handle up to 50 submissions per month for free. You could also easily switch out the end-point to use another contact form service.

Just swap the Formspree details in the ```_data/settings.yml``` file.

---

## Pretty cool, huh?

We've packed this theme with powerful features to show off your work.

Why not put them to use on your new website?