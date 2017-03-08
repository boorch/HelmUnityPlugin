/* Copyright 2017 Matt Tytel */

#include "helm_sequencer.h"

#define kDefaultNumSixteenths 16

namespace Helm {

  HelmSequencer::HelmSequencer() {
    channel_ = 0;
    num_sixteenths_ = kDefaultNumSixteenths;
  }

  HelmSequencer::~HelmSequencer() {
    for (auto note : on_events_)
      delete note.second;
    on_events_.clear();
    off_events_.clear();
  }

  HelmSequencer::Note* HelmSequencer::addNote(int midi_note, double start, double end) {
    Note* note = new Note();
    note->midi_note = midi_note;
    note->time_on = start;
    note->time_off = end;

    on_events_[std::pair<double, int>(start, midi_note)] = note;
    off_events_[std::pair<double, int>(end, midi_note)] = note;
    return note;
  }

  void HelmSequencer::deleteNote(Note* note) {
    on_events_.erase(std::pair<double, int>(note->time_on, note->midi_note));
    off_events_.erase(std::pair<double, int>(note->time_off, note->midi_note));
    delete note;
  }

  void HelmSequencer::changeNoteStart(Note* note, double start) {
    on_events_.erase(std::pair<double, int>(note->time_on, note->midi_note));
    note->time_on = start;
    on_events_[std::pair<double, int>(start, note->midi_note)] = note;
  }

  void HelmSequencer::changeNoteEnd(Note* note, double end) {
    off_events_.erase(std::pair<double, int>(note->time_off, note->midi_note));
    note->time_off = end;
    off_events_[std::pair<double, int>(end, note->midi_note)] = note;
  }

  void HelmSequencer::getNoteEvents(int* notes, event_map events, double start, double end) {
    auto iter = events.lower_bound(std::pair<double, int>(start, 0));

    int note_index = 0;
    while (iter != events.end() && (start > end || iter->first.first < end))
      notes[note_index++] = (*iter).first.second;

    if (start > end) {
      iter = events.lower_bound(std::pair<double, int>(0.0, 0));

      while (iter != events.end() && iter->first.first < end)
        notes[note_index++] = (*iter).first.second;
    }

    notes[note_index] = -1;
  }

  void HelmSequencer::getNoteOns(int* notes, double start, double end) {
    getNoteEvents(notes, on_events_, start, end);
  }

  void HelmSequencer::getNoteOffs(int* notes, double start, double end) {
    getNoteEvents(notes, off_events_, start, end);
  }
}
