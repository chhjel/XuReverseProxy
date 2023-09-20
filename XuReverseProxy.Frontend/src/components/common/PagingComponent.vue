<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import IdUtils from "@utils/IdUtils";

interface PageinationButton {
  number: number;
}

@Options({
  components: {},
})
export default class PagingComponent extends Vue {
  @Prop({ required: true })
  value!: number;

  @Prop({ required: false, default: 0 })
  count!: number;

  @Prop({ required: false, default: null })
  pagesCount!: number | null;

  @Prop({ required: false, default: 100 })
  pageSize!: number;

  @Prop({ required: false, default: false })
  asIndex!: boolean;

  @Prop({ required: false, default: false })
  hideIfSinglePage!: boolean;

  @Prop({ required: false, default: false })
  disabled!: boolean;

  id: string = IdUtils.generateId();
  currentValue: number = this.asIndex ? 0 : 1;

  //////////////////
  //  LIFECYCLE  //
  ////////////////
  mounted(): void {
    this.currentValue = this.value;
  }

  ////////////////
  //  GETTERS  //
  //////////////
  get visible(): boolean {
    if (this.hideIfSinglePage && this.pageCount < 2) return false;
    if (this.pagesCount != null) return this.pagesCount > 0;
    return this.count > 1 && this.pageCount > 1;
  }

  get pageCount(): number {
    if (this.pagesCount != null) return this.pagesCount;
    return Math.ceil(this.count / this.pageSize);
  }

  hasExtraButton: boolean = false;
  get buttons(): Array<PageinationButton> {
    let buttons: Array<PageinationButton> = [];
    this.hasExtraButton = false;

    for (let i = 0; i < this.pageCount; i++) {
      buttons.push({
        number: i + 1,
      });
    }

    return buttons;
  }

  ////////////////
  //  METHODS  //
  //////////////
  isActive(btn: PageinationButton): boolean {
    let num = btn.number;
    return this.asIndex ? num - 1 == this.currentValue : num == this.currentValue;
  }

  navigateToPage(num: number): void {
    if (this.asIndex) {
      num--;
    }

    const min = this.asIndex ? 0 : 1;
    const max = this.asIndex ? this.pageCount - 1 : this.pageCount;
    if (num < min) num = min;
    else if (num > max) num = max;

    this.$emit("update:value", num);
    this.$emit("change", num);
    this.currentValue = num;
  }

  ///////////////////////
  //  EVENT HANDLERS  //
  /////////////////////
  onClickedButton(btn: PageinationButton): void {
    if (this.disabled || this.isActive(btn)) return;
    this.navigateToPage(btn.number);
  }

  @Watch("value")
  onValueChanged(): void {
    this.currentValue = this.value;
  }
}
</script>

<template>
  <div class="paging-component" v-if="visible">
    <div
      v-for="(btn, bIndex) in buttons"
      :key="`page-btn-${bIndex}-${btn.number}-${id}`"
      @click="onClickedButton(btn)"
      class="page-button"
      :class="{ active: isActive(btn), disabled: disabled }"
    >
      <span>{{ btn.number }}</span>
    </div>
  </div>
</template>

<style scoped lang="scss">
.paging-component {
  display: flex;
  flex-wrap: nowrap;
  max-width: 100%;
  overflow-x: auto;
  padding-bottom: 5px;

  .page-button {
    display: inline-flex;
    align-items: center;
    padding: 5px 15px;
    user-select: none;
    background-color: var(--color--panel-light);
    border-radius: 0;
    margin-right: 5px;

    @media (max-width: 599px) {
      padding: 10px 25px;
    }

    &:not(.active) {
      cursor: pointer;
    }

    &.active {
      font-weight: 600;
      background-color: var(--color--secondary);
    }

    &.disabled {
      color: #bbb;
      cursor: default;
    }
  }
}
</style>
