<script setup lang="ts">
import { useWebSocketStore } from '@/stores/webSocketStore'

const store = useWebSocketStore()

</script>

<template>
  <div class="flex gap-2">
    <div v-for="user in store.users" class="bg-primary p-1 px-2 rounded-md"> {{ user }}</div>
  </div>
</template>
