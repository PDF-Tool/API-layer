<script setup lang="ts">
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuRadioGroup,
    DropdownMenuRadioItem,
    DropdownMenuTrigger,
} 
from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { fileSizeFormats, metrics } from '@/lib/utils'
import { pageFormats } from '@/lib/pageFormat'
import { Label } from '@/components/ui/label'
import { ref, computed } from 'vue'
import { usePdfStore } from '@/stores/pdfStore'

const { createAndPrintRandomPdf } = usePdfStore()

const mode = ref<'single' | 'batch'>('single')
const numberOfFiles = ref(2)
const sizeMin = ref(1)
const sizeMax = ref(10)
const pagesMin = ref(1)
const pagesMax = ref(10)
const byteUnit = ref<'MB' | 'GB'>('MB')
const metricUnit = ref('mm')
const format = ref('A4')
const width = ref<number | undefined>(undefined)
const height = ref<number | undefined>(undefined)

const canCreate = computed(() => {
    return sizeMin.value > 0 && sizeMax.value >= sizeMin.value && pagesMin.value > 0 && pagesMax.value >= pagesMin.value && (mode.value === 'single' || numberOfFiles.value > 0)
})

function getRandomInt(min: number, max: number) {
    return Math.floor(Math.random() * (max - min + 1)) + min
}

</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="flex gap-4 items-center">
            <Label>Mode:</Label>
            <Button :variant="mode === 'single' ? 'default' : 'outline'" @click="mode = 'single'">Single</Button>
            <Button :variant="mode === 'batch' ? 'default' : 'outline'" @click="mode = 'batch'">Batch</Button>
        </div>
        <div v-if="mode === 'batch'" class="grid w-full items-center gap-2">
            <Label for="numberOfFiles">Amount of documents</Label>
            <Input type="number" v-model="numberOfFiles" name="numberOfFiles" min="1" placeholder="Select amount of documents..." />
        </div>
        <div class="grid w-full items-center gap-2">
            <Label>Size per page (random range)</Label>
            <div class="flex gap-2">
                <Input type="number" v-model="sizeMin" min="1" placeholder="Min size..." />
                <Input type="number" v-model="sizeMax" min="1" placeholder="Max size..." />
                <DropdownMenu>
                    <DropdownMenuTrigger as-child>
                        <Button class="min-w-[70px] select-none"> {{ byteUnit }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                        <DropdownMenuRadioGroup v-model="byteUnit">
                            <DropdownMenuRadioItem v-for="format in fileSizeFormats" :key="format" :value="format">
                                {{ format }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </div>
        <div class="grid w-full items-center gap-2">
            <Label>Pages (random range)</Label>
            <div class="flex gap-2">
                <Input type="number" v-model="pagesMin" min="1" placeholder="Min pages..." />
                <Input type="number" v-model="pagesMax" min="1" placeholder="Max pages..." />
            </div>
        </div>
        <div class="p-4 bg-primary rounded-lg border-input border shadow-sm flex flex-col gap-4">
            <div class="flex flex-col w-full items-center gap-2">
                <label class="w-full">Preset</label>
                <DropdownMenu class="w-full">
                    <DropdownMenuTrigger as-child>
                        <Button :disabled="!!width || !!height"
                            class="select-none border-1 text-black  bg-background! justify-start hover:bg-transparent  w-full">
                            {{ format }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent class="w-full">
                        <DropdownMenuRadioGroup v-model="format">
                            <DropdownMenuRadioItem v-for="f in pageFormats" :key="f" :value="f">
                                {{ f }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
            <div class="grid w-full items-center gap-2">
                <Label for="dimensions">Dimensions</Label>
                <div class="flex gap-2">
                    <div class="relative">
                        <Input id="width" type="number" v-model="width" class="pl-7 bg-background!" placeholder="Width" />
                        <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                            <span class="text-foreground/50">W</span>
                        </span>
                    </div>
                    <div class="relative">
                        <Input id="height" type="number" v-model="height" class="pl-7 bg-background!" placeholder="Height" />
                        <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                            <span class="text-foreground/50">H</span>
                        </span>
                    </div>
                    <DropdownMenu>
                        <DropdownMenuTrigger as-child>
                            <Button class="min-w-[70px] select-none"> {{ metricUnit }}
                                <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent>
                            <DropdownMenuRadioGroup v-model="metricUnit">
                                <DropdownMenuRadioItem v-for="metric in metrics" :key="metric" :value="metric">
                                    {{ metric }}
                                </DropdownMenuRadioItem>
                            </DropdownMenuRadioGroup>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </div>
            </div>
        </div>
        <Button :disabled="!canCreate" @click="() => createAndPrintRandomPdf({
            sizeMin,
            sizeMax,
            pagesMin,
            pagesMax,
            mode,
            numberOfFiles,
            byteUnit,
            metricUnit
        })">Generate PDF(s) with Random Values</Button>
    </div>
</template>