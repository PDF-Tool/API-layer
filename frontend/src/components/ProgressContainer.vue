<script lang="ts" setup>
import { computed } from 'vue';
import Progress from './Progress.vue';
import { useWebSocketStore } from '@/stores/webSocketStore';
import CanonLogo from '@/assets/canon-logo.svg';

const ws = useWebSocketStore();

const processList = computed(() => Object.values(ws.processes));
const queue = computed(() =>
  processList.value.filter(p => p.progress && !p.completed && !p.failed)
);
const completed = computed(() =>
  processList.value.filter(p => p.completed)
);
const failed = computed(() =>
  processList.value.filter(p => p.failed)
);
</script>

<template>
  <div>
    <div>
      <h2 class="font-medium">Queue</h2>
      <div class="flex flex-col gap-4">
        <Progress
          v-for="p in queue"
          :key="p.progress?.ProcessId"
          :progress="p.progress?.PercentComplete || 0"
          :title="p.started?.ProcessName && p.started.ProcessName.includes('Random')
            ? `Random PDF Generation (by ${p.started?.Initiator || 'unknown'})`
            : p.started?.ProcessName && p.started.ProcessName.includes('Batch')
              ? `Batch PDF Generation (by ${p.started?.Initiator || 'unknown'})`
              : `${p.started?.ProcessName || 'PDF Generation'} (by ${p.started?.Initiator || 'unknown'})`"
        >
          <template #icon>
            <CanonLogo class="w-10 h-10 text-accent" />
          </template>
        </Progress>
      </div>
    </div>

    <div>
      <h2 class="font-medium ">Completed</h2>
      <div class="flex flex-col gap-4">
        <div v-for="p in completed" :key="p.completed?.ProcessId" class="p-2 rounded bg-green-100">
          <div class="font-semibold">
            <template v-if="p.started?.ProcessName && p.started.ProcessName.includes('Random')">
              Random PDF Generation (by {{ p.started?.Initiator || 'unknown' }})
            </template>
            <template v-else-if="p.started?.ProcessName && p.started.ProcessName.includes('Batch')">
              Batch PDF Generation (by {{ p.started?.Initiator || 'unknown' }})
            </template>
            <template v-else>
              {{ p.started?.ProcessName || 'PDF Generation' }} (by {{ p.started?.Initiator || 'unknown' }})
            </template>
          </div>
          <div>Process ID: {{ p.completed?.ProcessId }}</div>
          <div>Duration: {{ p.completed?.Duration }}</div>
          <div v-if="p.completed?.AdditionalData">
            <div v-for="(val, key) in p.completed.AdditionalData" :key="key">
              <template v-if="key === 'actualSize' || key === 'actualSizes'">
                {{ key }}:
                <span v-if="Array.isArray(val)">
                  {{ val.map(v => Number(v).toFixed(2)).join(', ') }} {{ p.completed.AdditionalData.byteUnit || '' }}
                </span>
                <span v-else>
                  {{ Number(val).toFixed(2) }} {{ p.completed.AdditionalData.byteUnit || '' }}
                </span>
              </template>
            </div>
          </div>
        </div>
        <div v-for="p in failed" :key="p.failed?.ProcessId" class="p-2 rounded bg-red-100">
          <div class="font-semibold">
            <template v-if="p.started?.ProcessName && p.started.ProcessName.includes('Random')">
              Failed: Random PDF Generation (by {{ p.started?.Initiator || 'unknown' }})
            </template>
            <template v-else-if="p.started?.ProcessName && p.started.ProcessName.includes('Batch')">
              Failed: Batch PDF Generation (by {{ p.started?.Initiator || 'unknown' }})
            </template>
            <template v-else>
              Failed: {{ p.started?.ProcessName || 'PDF Generation' }} (by {{ p.started?.Initiator || 'unknown' }})
            </template>
          </div>
          <div>Process ID: {{ p.failed?.ProcessId }}</div>
          <div>Error: {{ p.failed?.ErrorMessage }}</div>
        </div>
      </div>
    </div>
  </div>
</template>