/* Copyright 2013-2016 Matt Tytel
 *
 * helm is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * helm is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with helm.  If not, see <http://www.gnu.org/licenses/>.
 */

#include "text_look_and_feel.h"
#include "fonts.h"
#include "synth_slider.h"

TextLookAndFeel::TextLookAndFeel() {
  setColour(ComboBox::backgroundColourId, Colour(0xff212121));
  setColour(ComboBox::arrowColourId, Colour(0xff888888));
  setColour(ComboBox::outlineColourId, Colour(0xff888888));
  setColour(ComboBox::textColourId, Colour(0xffbbbbbb));
  setColour(Label::textColourId, Colour(0xffaaaaaa));
  setColour(ListBox::backgroundColourId, Colour(0xff212121));
  setColour(ListBox::textColourId, Colour(0xffaaaaaa));
}

void TextLookAndFeel::drawRotarySlider(Graphics& g, int x, int y, int width, int height,
                                       float slider_t, float start_angle, float end_angle,
                                       Slider& slider) {
  static const float text_percentage = 0.7f;

  bool active = true;
  SynthSlider* s_slider = dynamic_cast<SynthSlider*>(&slider);
  if (s_slider)
    active = s_slider->isActive();

  if (active)
    g.fillAll(Colour(0xff424242));
  else
    g.fillAll(Colour(0xff333333));

  if (active)
    g.setColour(Colour(0xff565656));
  else
    g.setColour(Colour(0xff333333));
  g.drawRect(slider.getLocalBounds());

  if (active)
    g.setColour(Colours::white);
  else
    g.setColour(Colour(0xff555555));

  g.setFont(Fonts::instance()->monospace().withPointHeight(height * text_percentage));
  g.drawText(slider.getTextFromValue(slider.getValue()),
             x, y, width, height, Justification::centred, false);
}

void TextLookAndFeel::drawToggleButton(Graphics& g, ToggleButton& button,
                                       bool isMouseOverButton, bool isButtonDown) {
  if (button.getToggleState())
    g.setColour(Colour(0xffffc400));
  else
    g.setColour(Colour(0xff313131));
  g.fillRect(button.getLocalBounds());

  g.setColour(Colours::white);
  g.setFont(Fonts::instance()->monospace());
  g.drawText(button.getButtonText(), 0, 0,
             button.getWidth(), button.getHeight(), Justification::centred);

  g.setColour(Colour(0xff565656));
  g.drawRect(button.getLocalBounds());

  if (isButtonDown) {
    g.setColour(Colour(0x11000000));
    g.fillRect(button.getLocalBounds());
  }
  else if (isMouseOverButton) {
    g.setColour(Colour(0x11ffffff));
    g.fillRect(button.getLocalBounds());
  }
}

void TextLookAndFeel::drawTickBox(Graphics& g, Component& component,
                                  float x, float y, float w, float h, bool ticked,
                                  bool enabled, bool mouse_over, bool button_down) {
  float border_width = 1.5f;
  g.setColour(Colour(0xffbbbbbb));
  g.drawRect(x + border_width, y + border_width,
             w - 2 * border_width, h - 2 * border_width, border_width);

  if (ticked) {
    g.setColour(Colour(0xffffd740));
    g.fillRect(x + 3 * border_width, y + 3 * border_width,
               w - 6 * border_width, h - 6 * border_width);
  }
}
