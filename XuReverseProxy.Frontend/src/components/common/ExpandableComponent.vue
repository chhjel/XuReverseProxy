<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class ExpandableComponent extends Vue {
    @Prop()
    header: string;

    @Prop({ required: false, default: false })
    startOpen: boolean;

    isOpen: boolean = false;

    mounted(): void {
        this.isOpen = this.startOpen;
    }

    get rootClasses(): any {
        let classes: any = {};
        classes['open'] = this.isOpen;
        return classes;
    }
}
</script>

<template>
    <div class="expandable" :class="rootClasses">
        <div class="expandable__header" @click="isOpen = !isOpen">{{ header }}</div>
        <div class="expandable__content" v-show="isOpen">
            <slot></slot>
        </div>
    </div>
</template>

<style scoped lang="scss">
.expandable {
    transition: all 0.2s;
    display: inline-block;
    border: 2px solid var(--color--panel);

    &.open {
        margin: 5px;
        display: block;
    }
    &__header {
        color: var(--color--text-dark);
        font-weight: 600;
        padding: 10px;
        user-select: none;
        cursor: pointer;
        &.open {
            margin-bottom: 5px;
            padding: 15px;
        }
        &:hover {
            background: var(--color--panel);
        }
    }
    &__content {
        padding: 10px;
    }
}
</style>
