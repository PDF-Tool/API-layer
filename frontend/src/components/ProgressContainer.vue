<script lang="ts" setup>
import { computed } from 'vue';
import Progress from './Progress.vue';
import { useWebSocketStore } from '@/stores/webSocketStore';

const ws = useWebSocketStore();

const processList = computed(() => Object.values(ws.processes));
const queue = computed(() =>
  processList.value.filter(p => p.progress && !p.completed && !p.failed)
);
const completed = computed(() =>
  processList.value.filter(p => p.completed)
);
</script>

<template>
  <div>
    <div>
      <h2 class="font-medium">Queue</h2>
      <div class="flex flex-col gap-4">
        <Progress v-for="p in queue" :key="p.progress?.ProcessId" :progress="p.progress?.PercentComplete || 0" :title="p.started?.ProcessName && p.started.ProcessName.includes('Random')
          ? `Random PDF Generation (by ${p.started?.Initiator || 'unknown'})`
          : p.started?.ProcessName && p.started.ProcessName.includes('Batch')
            ? `Batch PDF Generation (by ${p.started?.Initiator || 'unknown'})`
            : `${p.started?.ProcessName || 'PDF Generation'} (by ${p.started?.Initiator || 'unknown'})`">
          <template #icon>
            <!-- <CanonLogo class="w-10 h-10 text-accent" /> -->
          </template>
        </Progress>
      </div>
    </div>

    <div>
      <h2 class="font-medium ">Completed</h2>
      <div class="flex flex-col gap-4">
        <Progress v-for="p in completed" :key="p.progress?.ProcessId" :progress="100" :title="p.started?.ProcessName && p.started.ProcessName.includes('Random')
          ? `Random PDF Generation (by ${p.started?.Initiator || 'unknown'})`
          : p.started?.ProcessName && p.started.ProcessName.includes('Batch')
            ? `Batch PDF Generation (by ${p.started?.Initiator || 'unknown'})`
            : `${p.started?.ProcessName || 'PDF Generation'} (by ${p.started?.Initiator || 'unknown'})`">
          <template #icon>
            <!-- <CanonLogo class="w-10 h-10 text-accent" /> -->
          </template>
        </Progress>
      </div>
    </div>
  </div>
</template>