<script setup lang="ts">
import { useWebSocketStore } from '@/stores/webSocketStore'
import { toSvg } from "jdenticon";

const store = useWebSocketStore()

const getUrl = (name: string) => {
  const svgString = toSvg(name, 100);
  return 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svgString);
}
</script>

<template>
  <div class="flex items-center gap-4">
    <div class="flex">
      <div v-for="(user, index) in store.users" :key="user" class="flex flex-col items-center" :style="{ marginLeft: index > 0 ? '-0.75rem' : '0' }">
        <div
          class="bg-primary p-1 rounded-full w-10 h-10 flex items-center justify-center border-2 border-white relative hover:z-10 transition-transform hover:scale-110"
          :class="{ 'ring-2 ring-accent': user === store.username }"
        >
          <img :src="getUrl(user)" alt="avatar" class="w-full h-full object-contain">
        </div>
        <span v-if="user === store.username" class="mt-1 text-xs text-accent font-bold">You</span>
      </div>
    </div>
    <span v-if="store.username" class="font-semibold text-accent">{{ store.username }}</span>
  </div>
</template>