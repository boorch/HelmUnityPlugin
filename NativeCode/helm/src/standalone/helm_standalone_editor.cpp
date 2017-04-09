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

#include "helm_standalone_editor.h"

#include "default_look_and_feel.h"
#include "helm_common.h"
#include "load_save.h"
#include "mopo.h"
#include "startup.h"
#include "utils.h"

#define WIDTH 992
#define HEIGHT 734
#define MAX_OUTPUT_MEMORY 1048576
#define MAX_BUFFER_PROCESS 256

HelmStandaloneEditor::HelmStandaloneEditor() : SynthGuiInterface(this) {
  computer_keyboard_ = new HelmComputerKeyboard(&engine_, keyboard_state_);

  setAudioChannels(0, mopo::NUM_CHANNELS);

  AudioDeviceManager::AudioDeviceSetup setup;
  deviceManager.getAudioDeviceSetup(setup);
  setup.sampleRate = mopo::DEFAULT_SAMPLE_RATE;
  deviceManager.initialise(0, mopo::NUM_CHANNELS, nullptr, true, "", &setup);

  if (deviceManager.getCurrentAudioDevice() == nullptr) {
    const OwnedArray<AudioIODeviceType>& device_types = deviceManager.getAvailableDeviceTypes();

    for (AudioIODeviceType* device_type : device_types) {
      deviceManager.setCurrentAudioDeviceType(device_type->getTypeName(), true);
      if (deviceManager.getCurrentAudioDevice())
        break;
    }
  }

  const StringArray all_midi_ins(MidiInput::getDevices());

  for (int i = 0; i < all_midi_ins.size(); ++i)
    deviceManager.setMidiInputEnabled(all_midi_ins[i], true);

  deviceManager.addMidiInputCallback("", midi_manager_);

  setLookAndFeel(DefaultLookAndFeel::instance());
  addAndMakeVisible(gui_);
  gui_->setOutputMemory(getOutputMemory());
  gui_->animate(LoadSave::shouldAnimateWidgets());
  setSize(WIDTH, HEIGHT);

  setWantsKeyboardFocus(true);
  grabKeyboardFocus();
  addKeyListener(computer_keyboard_);
  postMessage(new Message());
  setOpaque(true);
}

HelmStandaloneEditor::~HelmStandaloneEditor() {
  shutdownAudio();
  midi_manager_ = nullptr;
  computer_keyboard_ = nullptr;
  gui_ = nullptr;
  keyboard_state_ = nullptr;
}

void HelmStandaloneEditor::prepareToPlay(int buffer_size, double sample_rate) {
  engine_.setSampleRate(sample_rate);
  engine_.setBufferSize(std::min(buffer_size, MAX_BUFFER_PROCESS));
  midi_manager_->setSampleRate(sample_rate);
}

void HelmStandaloneEditor::getNextAudioBlock(const AudioSourceChannelInfo& buffer) {
  ScopedLock lock(getCriticalSection());

  int num_samples = buffer.buffer->getNumSamples();
  int synth_samples = std::min(num_samples, MAX_BUFFER_PROCESS);

  processControlChanges();
  processModulationChanges();
  MidiBuffer midi_messages;
  midi_manager_->removeNextBlockOfMessages(midi_messages, num_samples);
  processMidi(midi_messages);
  processKeyboardEvents(midi_messages, num_samples);

  for (int b = 0; b < num_samples; b += synth_samples) {
    int current_samples = std::min<int>(synth_samples, num_samples - b);

    processAudio(buffer.buffer, mopo::NUM_CHANNELS, current_samples, b);
  }
}

void HelmStandaloneEditor::releaseResources() {
}

void HelmStandaloneEditor::paint(Graphics& g) {
}

void HelmStandaloneEditor::resized() {
  Rectangle<int> bounds = getBounds();
  double scale = std::min(double(bounds.getWidth()) / WIDTH, double(bounds.getHeight()) / HEIGHT);
  gui_->setTransform(AffineTransform::scale(scale));
  gui_->setBounds(Rectangle<int>(0, 0, bounds.getWidth() / scale, bounds.getHeight() / scale));
}

void HelmStandaloneEditor::handleMessage(const Message& message) {
#ifdef JUCE_MAC
  Thread::sleep(100);
  if(!hasKeyboardFocus(false)) {
    postMessage(new Message());
    grabKeyboardFocus();
  }
#endif
}
