<script lang="ts">
import { LoadStatus } from "@services/ServiceBase";
import ValueUtils from "@utils/ValueUtils";
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'

@Options({
    components: {  }
})
export default class LoaderComponent extends Vue {
    @Prop({ required: false, default: null })
    value: boolean | null;

    @Prop({ required: false, default: null })
    error: string | null;

    @Prop({ required: false, default: null })
    allow: boolean | null;

    @Prop({ required: false, default: null })
    status: LoadStatus | Array<LoadStatus>;

    @Prop({ required: false, default: null })
    forId: string | null;

    @Prop({ required: false, default: false })
    inline!: string | boolean;

    @Prop({ required: false, default: '-9px' })
    inlineYAdjustment!: string;

    // async mounted() {}

    get isInline(): boolean { return ValueUtils.IsToggleTrue(this.inline); }

    get isVisible(): boolean {
        if (this.allow === false) return false;
        else if (this.value === true) return true;
        else if (this.value === false) return false;
        else if (this.status != null
            && !Array.isArray(this.status)
            && this.forId != null
            && this.forId != this.status.id) return false;
        else if (this.showError) return true;
        else if (Array.isArray(this.status) 
            && this.status.length > 0 
            && this.status.some(x => x.inProgress || x.failed)) return true;
        else if (this.status != null 
            && !Array.isArray(this.status)
            && (this.status.inProgress || this.status.failed)) return true;
        else return false;
    }

    get showLoader(): boolean {
        if (this.value === true) return true;
        else if (Array.isArray(this.status) 
            && this.status.length > 0 
            && this.status.some(x => x.inProgress)) return true;
        else if (this.status != null 
            && !Array.isArray(this.status)
            && (this.status.inProgress)) return true;
        else return false;
    }

    get showError(): boolean {
        return this.errorMessage != null 
            && this.errorMessage.length > 0;
    }

    get errorMessage(): string | null {
        if (this.error) return this.error;
        else if (Array.isArray(this.status)) return this.status.find(x => x.failed)?.errorMessage;
        else if (!Array.isArray(this.status)) return this.status.errorMessage;
        else return null;
    }

    get rootClasses(): any {
        let classes: any = {};
        classes['inline'] = this.isInline;
        return classes;
    }
}
</script>

<template>
    <div class="loader" v-if="isVisible" :class="rootClasses">
        <div v-if="showLoader" class="loader__spinner">
            <div class="spinner-container" :style="{ bottom: inlineYAdjustment }">
                <div class="spinner-dot spinner-dot-1"></div>
                <div class="spinner-dot spinner-dot-2"></div>
                <div class="spinner-dot spinner-dot-3"></div>
            </div>
        </div>
        <div v-if="showError" class="loader__error">
            {{ errorMessage }}
        </div>
    </div>
</template>

<style scoped lang="scss">
.loader {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    animation: 0.5s fadeIn;

    &__spinner {
        height: 48px;
        display: flex;
        justify-content: center;
        align-items: center;

        .spinner-container {
            width: 120px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .spinner-dot {
            width: 20px;
            height: 20px;
            border-radius: 50%;
            background-color: var(--color--secondary);
        }

        .spinner-dot-1 {
            animation: pulse .4s ease 0s infinite alternate;
        }
        .spinner-dot-2 {
            animation: pulse .4s ease .2s infinite alternate;
        }
        .spinner-dot-3 {
            animation: pulse .4s ease .4s infinite alternate;
        }
    }

    &.inline {
        display: inline-flex;
        .loader__spinner {
            height: inherit;
        }

        .spinner-container {
            width: 76px;
            position: relative;
        }

        .spinner-dot {
            width: 16px;
            height: 16px;
        }
    }

    &__error {
        border: 2px solid var(--color--danger);
        padding: 8px;
        color: var(--color--danger-lighten);
        font-family: monospace;
        font-size: 14px;
        max-width: 600px;
    }
        
    @keyframes pulse {
        from {
            opacity: 1;
            transform: scale(1);
        }
        to {
            opacity: .25;
            transform: scale(.75);
        }
    }

    @keyframes fadeIn {
        0% {
            opacity: 0;
        }
        100% {
            opacity: 1;
        }
    }
}
</style>
