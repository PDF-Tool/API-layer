<script setup lang="ts">
import { useWebSocketStore } from '@/stores/webSocket'

const store = useWebSocketStore()
// defineProps<{
//   msg: string
// }>()
</script>

<template>
  <div class="greetings">
    <h1 class="green">{{ store.users }}</h1>
  </div>
</template>
