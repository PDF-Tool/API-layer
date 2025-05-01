<script lang="ts" setup>
import { Button } from '@/components/ui/button'

defineProps<{
    progress: number
    title: string
}>()
</script>

<template>
    <div class="flex gap-2 p-2 rounded-lg items-center">
        <div class="flex items-center justify-center w-10 h-10">
          <slot name="icon">
            <span class="icon-[mdi--folder] text-accent text-6xl"></span>
          </slot>
        </div>
        <div class="flex flex-col flex-1 justify-center">
            <div class="flex items-center justify-between mb-1">
                <span class="text-sm font-medium">{{ title }}</span>
                <span class="text-sm font-medium  transition-all duration-300 w-12 text-right">{{ progress
                    }}%</span>
            </div>
            <div class="w-full">
                <div class="h-2 w-full rounded-full bg-primary overflow-hidden">
                    <div class="h-full bg-accent rounded-full transition-all duration-500 ease-out"
                        :style="{ width: `${progress}%` }"></div>
                </div>
            </div>
        </div>
        <div class="flex items-center">
            <Button size="icon" variant="ghost" class="flex items-center">
                <span class="icon-[mdi--cancel-circle] bg-ring text-lg"></span>
            </Button>
        </div>
    </div>
</template>