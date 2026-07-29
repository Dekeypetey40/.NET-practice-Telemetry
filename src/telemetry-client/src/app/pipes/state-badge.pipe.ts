import { Pipe, PipeTransform } from '@angular/core';
import { RunState } from '../models/run.model';

export interface StateBadge {
  label: string;
  color: 'primary' | 'accent' | 'warn' | undefined;
  cssClass: string;
}

const STATE_MAP: Record<RunState, StateBadge> = {
  Created: { label: 'Created', color: undefined, cssClass: 'badge-created' },
  Queued: { label: 'Queued', color: 'primary', cssClass: 'badge-queued' },
  Running: { label: 'Running', color: 'accent', cssClass: 'badge-running' },
  Completed: { label: 'Completed', color: undefined, cssClass: 'badge-completed' },
  Failed: { label: 'Failed', color: 'warn', cssClass: 'badge-failed' },
  Canceled: { label: 'Canceled', color: undefined, cssClass: 'badge-canceled' },
};

/**
 * Transforms a RunState string into a StateBadge object for use with Material chips.
 * Usage: `run.currentState | stateBadge` returns { label, color, cssClass }.
 */
@Pipe({ name: 'stateBadge', standalone: true, pure: true })
export class StateBadgePipe implements PipeTransform {
  transform(state: RunState): StateBadge {
    return STATE_MAP[state] ?? { label: state, color: undefined, cssClass: 'badge-unknown' };
  }
}
