<script setup lang="ts">
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuRadioGroup,
    DropdownMenuRadioItem,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { fileSizeFormats, metrics } from '@/lib/utils'
import { pageFormats } from '@/lib/pageFormat'
import { Label } from '@/components/ui/label'
import { computed } from 'vue'
import { usePdfStore } from '@/stores/pdfStore'

const { batchFormData, createAndPrintBatchPdf } = usePdfStore()

const canCreateBatchPdf = computed(() => {
    return batchFormData.NumberOfFiles > 0 && batchFormData.PagesPerFile > 0 && batchFormData.SizePerPage > 0
})
</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="grid w-full items-center gap-2">
            <Label for="numberOfFiles">Amount of documents</Label>
            <Input type="number" v-model="batchFormData.NumberOfFiles" name="numberOfFiles" placeholder="Select amount of documents..." />
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="sizePerPage">Size per page</Label>
            <div class="flex gap-2">
                <Input type="number" v-model="batchFormData.SizePerPage" name="sizePerPage" placeholder="Select image size per page..." />
                <DropdownMenu>
                    <DropdownMenuTrigger as-child>
                        <Button class="min-w-[70px] select-none"> {{ batchFormData.ByteUnit }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                        <DropdownMenuRadioGroup v-model="batchFormData.ByteUnit">
                            <DropdownMenuRadioItem v-for="format in fileSizeFormats" :key="format" :value="format">
                                {{ format }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="pagesPerFile">Amount of pages per document</Label>
            <Input type="number" name="pagesPerFile" v-model="batchFormData.PagesPerFile" placeholder="Select amount of pages..." />
        </div>
        <div class="p-4 bg-primary rounded-lg border-input border shadow-sm flex flex-col gap-4">
            <div class="flex flex-col w-full items-center gap-2">
                <label class="w-full">Preset</label>
                <DropdownMenu class="w-full">
                    <DropdownMenuTrigger as-child>
                        <Button class="select-none border-1 text-black bg-background! justify-start hover:bg-transparent w-full">
                            {{ batchFormData.Format || 'A4' }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent class="w-full">
                        <DropdownMenuRadioGroup v-model="batchFormData.Format">
                            <DropdownMenuRadioItem v-for="format in pageFormats" :key="format" :value="format">
                                {{ format }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
            <div class="grid w-full items-center gap-2">
                <Label for="dimensions">Dimensions</Label>
                <div class="flex gap-2">
                    <div class="relative">
                        <Input id="width" type="number" v-model="batchFormData.Width" class="pl-7 bg-background!" placeholder="Width" />
                        <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                            <span class="text-foreground/50">W</span>
                        </span>
                    </div>
                    <div class="relative">
                        <Input id="height" type="number" v-model="batchFormData.Height" class="pl-7 bg-background!" placeholder="Height" />
                        <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                            <span class="text-foreground/50">H</span>
                        </span>
                    </div>
                    <DropdownMenu>
                        <DropdownMenuTrigger as-child>
                            <Button class="min-w-[70px] select-none"> {{ batchFormData.MetricUnit }}
                                <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent>
                            <DropdownMenuRadioGroup v-model="batchFormData.MetricUnit">
                                <DropdownMenuRadioItem v-for="metric in metrics" :key="metric" :value="metric">
                                    {{ metric }}
                                </DropdownMenuRadioItem>
                            </DropdownMenuRadioGroup>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </div>
            </div>
        </div>
        <Button :disabled="!canCreateBatchPdf" @click="createAndPrintBatchPdf">Generate Batch PDFs</Button>
    </div>
</template>