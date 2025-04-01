<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <header class="pb-8">
    <img alt="Vue logo" src="@/assets/canon.svg" width="190" height="125" />
  </header>
  <div class="grid grid-cols-2 gap-4">
    <div class="w-full">
      <RouterView />
    </div>
    <div class="w-full">

    </div>
  </div>
</template>