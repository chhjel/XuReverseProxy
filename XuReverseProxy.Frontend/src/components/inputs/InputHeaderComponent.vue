<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Vue } from 'vue-property-decorator'

@Options({
	components: { }
})
export default class InputHeaderComponent extends Vue {
    @Prop({ required: false, default: '' })
    label: string;

    @Prop({ required: false, default: '' })
    description: string;

    showDescription: boolean = false;
}
</script>

<template>
  <div class="input-header">
    <div class="input-header__mainrow">
        <label v-if="label" class="input-header__label">{{ label }}</label>
        <div v-if="description" class="input-header__description-icon" @click="showDescription = !showDescription">
            <div class="material-icons icon">question_mark</div>
        </div>
    </div>
    <div v-if="showDescription" class="input-header__description">{{ description }}</div>
  </div>
</template>

<style scoped lang="scss">
.input-header {
    &__mainrow {
        display: flex;
        flex-wrap: nowrap;
        align-items: center;
    }
    /* &__label { } */
    &__description-icon {
        transition: all 0.05s;
        margin-left: 10px;
        border: 1px solid var(--color--info-base);
        color: var(--color--info-base);
        width: 16px;
        height: 16px;
        text-align: center;
        border-radius: 7px;
        margin-bottom: 1px;
        cursor: pointer;
        user-select: none;

        .icon {
            font-size: 14px;
        }

        &:hover {
            background: var(--color--info-base);
            color: #000;
        }
    }
    &__description {
        color: var(--color--text-darker);
        padding: 5px;
        font-size: 14px;
        border-left: 2px solid #21292d;
        margin-left: 5px;
        margin-bottom: 5px;
    }
}
</style>
